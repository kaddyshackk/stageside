using ComedyPull.Application.Features.DataProcessing.Events;
using ComedyPull.Application.Features.DataProcessing.Interfaces;
using ComedyPull.Application.Features.DataProcessing.Services;
using ComedyPull.Domain.Models.Processing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Features.DataProcessing.Handlers
{
    public class StateCompletedHandler(IServiceProvider serviceProvider, ILogger<StateCompletedHandler> logger)
        : INotificationHandler<StateCompletedEvent>
    {
        public async Task Handle(StateCompletedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var nextState = ProcessingStateMachine.GetNextState(notification.CompletedState);
                var processor = GetProcessorForState(nextState);
            
                logger.LogInformation("Starting {NextStage} processing for batch {BatchId}", 
                    nextState, notification.BatchId);
            
                await processor.ProcessBatchAsync(notification.BatchId, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // No next stage - processing complete
                logger.LogInformation("Batch {BatchId} processing completed", notification.BatchId);
            }
        }

        private ITransformProcessor GetProcessorForState(ProcessingState stage)
        {
            return stage switch
            {
                ProcessingState.Transformed => (serviceProvider.GetService(typeof(ITransformProcessor)) as ITransformProcessor)
                    ?? throw new ArgumentException($"ITransformProcessor not registered in DI container"),

                _ => throw new ArgumentException($"No processor found for stage {stage}")
            };
        }
    }
}