using ComedyPull.Application.Modules.DataProcessing.Processors.Interfaces;
using ComedyPull.Domain.Models.Processing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Modules.DataProcessing
{
    /// <summary>
    /// Event handler for StateCompletedEvent. Starts the next stage in the data pipeline.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public class StateCompletedHandler(IServiceProvider serviceProvider, ILogger<StateCompletedHandler> logger)
        : INotificationHandler<StateCompletedEvent>
    {
        /// <summary>
        /// Handles the StateCompletedEvent.
        /// </summary>
        /// <param name="notification"><see cref="StateCompletedEvent"/> to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
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

        /// <summary>
        /// Gets the processor for the current <see cref="ProcessingState"/>.
        /// </summary>
        /// <param name="state">The state to fetch a <see cref="ITransformProcessor"/> for.</param>
        /// <returns>The <see cref="ITransformProcessor"/> that handles the provided state.</returns>
        /// <exception cref="ArgumentException">Thrown if no processor is registered for a state. Or if the specified state does not exist.</exception>
        private ITransformProcessor GetProcessorForState(ProcessingState state)
        {
            return state switch
            {
                ProcessingState.Transformed => (serviceProvider.GetService(typeof(ITransformProcessor)) as ITransformProcessor)
                    ?? throw new ArgumentException($"ITransformProcessor not registered in DI container"),
                _ => throw new ArgumentException($"No processor found for stage {state}")
            };
        }
    }
}