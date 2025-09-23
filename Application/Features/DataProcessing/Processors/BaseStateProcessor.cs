using ComedyPull.Application.Features.DataProcessing.Events;
using ComedyPull.Application.Features.DataProcessing.Interfaces;
using ComedyPull.Domain.Models.Processing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Features.DataProcessing.Processors
{
    public abstract class BaseStateProcessor(IMediator mediator, ILogger<BaseStateProcessor> logger)
        : IStateProcessor<ProcessingState>
    {
        public abstract ProcessingState FromState { get; }
        public abstract ProcessingState ToState { get; }

        public async Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Stage} processing for batch {BatchId}", ToState, batchId);
        
            try
            {
                await ProcessRecordsAsync(batchId, cancellationToken);
                await mediator.Publish(new StateCompletedEvent(batchId, ToState), cancellationToken);
            
                logger.LogInformation("Completed {Stage} processing for batch {BatchId}", ToState, batchId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed {Stage} processing for batch {BatchId}", ToState, batchId);
                throw;
            }
        }

        protected abstract Task ProcessRecordsAsync(Guid batchId, CancellationToken cancellationToken);
    }
}