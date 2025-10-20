using ComedyPull.Application.Modules.DataProcessing;
using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Events
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
            logger.LogInformation("StateCompletedHandler received event for batch {BatchId} with state {CompletedState}", 
                notification.BatchId, notification.CompletedState);
            
            try
            {
                var nextState = ProcessingStateMachine.GetNextState(notification.CompletedState);

                logger.LogInformation("Next state determined as {NextState} for batch {BatchId}", 
                    nextState, notification.BatchId);

                // Find state processor that handles this transition
                var processor = stateProcessors.FirstOrDefault(p =>
                    p.FromState == notification.CompletedState &&
                    p.ToState == nextState);

                if (processor == null)
                {
                    logger.LogError("No state processor found for transition {FromState} -> {ToState} for batch {BatchId}", 
                        notification.CompletedState, nextState, notification.BatchId);
                    throw new InvalidOperationException(
                        $"No state processor found for transition {notification.CompletedState} -> {nextState}");
                }

                logger.LogInformation("Found processor {ProcessorType} for transition {FromState} -> {ToState}. Starting {NextStage} processing for batch {BatchId}",
                    processor.GetType().Name, notification.CompletedState, nextState, nextState, notification.BatchId);

                await processor.ProcessBatchAsync(notification.BatchId, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                // No next stage - processing complete
                logger.LogInformation("✅ Batch {BatchId} processing pipeline completed successfully! Final state: {CompletedState}. (Exception: {Message})", 
                    notification.BatchId, notification.CompletedState, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing StateCompletedEvent for batch {BatchId} with state {CompletedState}", 
                    notification.BatchId, notification.CompletedState);
                throw;
            }
        }
    }
}