using ComedyPull.Application.Events;
using ComedyPull.Application.Modules.DataProcessing.Exceptions;
using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Services.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Transform.Interfaces;
using ComedyPull.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace ComedyPull.Application.Modules.DataProcessing.Steps.Transform
{
    public class TransformStateProcessor(
        IBatchRepository batchRepository,
        ITransformStateRepository repository,
        ISubProcessorResolver subProcessorResolver,
        IMediator mediator,
        ILogger<TransformStateProcessor> logger) : IStateProcessor
    {
        public ProcessingState FromState => ProcessingState.Ingested;
        public ProcessingState ToState => ProcessingState.Transformed;

        public async Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Stage} processing for batch {BatchId}", ToState, batchId);
            try
            {
                var batch = await batchRepository.GetBatchById(batchId, cancellationToken);
                if (batch.State != FromState)
                {
                    throw new InvalidBatchStateException(batchId, FromState, batch.State);
                }

                using (LogContext.PushProperty("@Batch", batch))
                using (LogContext.PushProperty("FromState", FromState))
                using (LogContext.PushProperty("ToState", ToState))
                {
                    var records = await repository.GetBronzeRecordsByBatchId(batchId, cancellationToken);

                    using (LogContext.PushProperty("BatchSize", records.Count()))
                    {
                        logger.LogInformation("Processing batch {BatchId} from {FromState} to {ToState}",
                            batch.Id, FromState, ToState);

                        // Resolve processor and start processing
                        var subProcessor = subProcessorResolver.Resolve(batch.SourceType, FromState, ToState);
                        await subProcessor.ProcessAsync(records, cancellationToken);

                        // Update batch state and signal state completed
                        await batchRepository.UpdateBatchStateById(batchId, ToState, cancellationToken);

                        await mediator.Publish(new StateCompletedEvent(batchId), cancellationToken);

                        logger.LogInformation("Completed {Stage} processing for batch {BatchId}", ToState, batchId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed {Stage} processing for batch {BatchId}", ToState, batchId);
                await batchRepository.UpdateBatchStateById(batchId, ProcessingState.Failed, cancellationToken);
                throw;
            }
        }
    }
}