using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.DataProcessing.Exceptions;
using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Services.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Transform.Interfaces;
using ComedyPull.Domain.Modules.DataProcessing;
using MediatR;
using Microsoft.Extensions.Logging;

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
                // 1. Load and validate batch
                var batch = await batchRepository.GetBatchById(batchId, cancellationToken);
                if (batch.State != FromState)
                {
                    throw new InvalidBatchStateException(batchId.ToString(), FromState, batch.State);
                }

                // 2. Load all bronze records for this batch
                var bronzeRecords = await repository.GetBronzeRecordsByBatchId(batchId, cancellationToken);

                logger.LogInformation("Processing {Count} records of type {SourceType} in batch {BatchId}",
                    bronzeRecords.Count(), batch.SourceType, batchId);

                // 3. Resolve SubProcessor by batch.SourceType (all records in batch have same type)
                var subProcessor = subProcessorResolver.Resolve(batch.SourceType, FromState, ToState);

                // 4. Process all records - SubProcessor will create SilverRecords
                await subProcessor.ProcessAsync(bronzeRecords, cancellationToken);

                await batchRepository.UpdateBatchStateById(batchId, ToState, cancellationToken);

                // 5. Publish completion event to trigger next stage
                await mediator.Publish(new StateCompletedEvent(batchId, ToState), cancellationToken);

                logger.LogInformation("Completed {Stage} processing for batch {BatchId}", ToState, batchId);
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