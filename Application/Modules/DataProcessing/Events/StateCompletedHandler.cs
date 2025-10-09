using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Modules.DataProcessing.Events
{
    /// <summary>
    /// Event handler for StateCompletedEvent. Starts the next stage in the data pipeline.
    /// </summary>
    public class StateCompletedHandler(
        IEnumerable<IStateProcessor> stateProcessors,
        ILogger<StateCompletedHandler> logger)
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

                // Find state processor that handles this transition
                var processor = stateProcessors.FirstOrDefault(p =>
                    p.FromState == notification.CompletedState &&
                    p.ToState == nextState);

                if (processor == null)
                {
                    throw new InvalidOperationException(
                        $"No state processor found for transition {notification.CompletedState} -> {nextState}");
                }

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
    }
}