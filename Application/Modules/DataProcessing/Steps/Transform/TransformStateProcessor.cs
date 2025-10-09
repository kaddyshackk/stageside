using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.DataProcessing.Services.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Transform.Interfaces;
using ComedyPull.Domain.Models.Processing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Modules.DataProcessing.Steps.Transform
{
    public class TransformStateProcessor(
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
                var records = await repository.GetRecordsByBatchAsync(batchId.ToString(), cancellationToken);
                var recordsBySource = records.GroupBy(r => r.Source);

                foreach (var group in recordsBySource)
                {
                    logger.LogInformation("Processing {Count} records for source {Source} in batch {BatchId}",
                        group.Count(), group.Key, batchId);

                    // Delegate to sub-processor for this source
                    var subProcessor = subProcessorResolver.Resolve(group.Key, FromState, ToState);
                    await subProcessor.ProcessAsync(group, cancellationToken);
                }
                await repository.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Saved changes for {RecordCount} records in batch {BatchId}", records.Count(), batchId);

                await mediator.Publish(new StateCompletedEvent(batchId, ToState), cancellationToken);

                logger.LogInformation("Completed {Stage} processing for batch {BatchId}", ToState, batchId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed {Stage} processing for batch {BatchId}", ToState, batchId);
                throw;
            }
        }
    }
}