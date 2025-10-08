using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.DataProcessing.Processors.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Repositories.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Services.Interfaces;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Models.Processing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Modules.DataProcessing.Processors
{
    public class TransformStateProcessor(
        ISourceRecordRepository recordRepository,
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
                // Pull records from DB
                var records = await recordRepository.GetRecordsByBatchAsync(batchId.ToString(), cancellationToken);

                // Group by source
                var recordsBySource = records.GroupBy(r => r.Source);

                foreach (var group in recordsBySource)
                {
                    logger.LogInformation("Processing {Count} records for source {Source} in batch {BatchId}",
                        group.Count(), group.Key, batchId);

                    // Resolve sub-processor for this source
                    var subProcessor = subProcessorResolver.Resolve(group.Key, FromState, ToState);

                    // Delegate to sub-processor
                    await subProcessor.ProcessAsync(group, cancellationToken);
                }

                // Move to next state
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