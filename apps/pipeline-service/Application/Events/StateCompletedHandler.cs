using ComedyPull.Application.Modules.DataProcessing;
using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace ComedyPull.Application.Events
{
    /// <summary>
    /// Event handler for StateCompletedEvent. Starts the next stage in the data pipeline.
    /// </summary>
    public class StateCompletedHandler(
        IBatchRepository batchRepository,
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
            using (LogContext.PushProperty("BatchId", notification.BatchId))
            {
                logger.LogInformation("Handling state completed event");
            
                var batch = await batchRepository.GetBatchById(notification.BatchId, cancellationToken);

                using (LogContext.PushProperty("CurrentState", batch.State))
                {
                    if (batch.State == ProcessingState.Completed)
                    {
                        logger.LogInformation("Batch processing already completed, no further action needed");
                        return;
                    }
                
                    var targetState = ProcessingStateMachine.GetNextState(batch.State);

                    if (targetState == null)
                    {
                        logger.LogCritical("State machine configuration error: no next state defined for non-terminal state");
                        throw new InvalidOperationException(
                            $"State machine misconfiguration: no next state defined for {batch.State}");
                    }

                    using (LogContext.PushProperty("TargetState", targetState))
                    {
                        var processor = stateProcessors.FirstOrDefault(p =>
                            p.FromState == batch.State &&
                            p.ToState == targetState);

                        if (processor == null)
                        {
                            logger.LogCritical("Processor configuration error: no processor registered for state transition");
                            throw new InvalidOperationException(
                                $"No state processor registered for transition {batch.State} -> {targetState}");
                        }

                        logger.LogInformation("Starting state transition with processor {ProcessorType}", 
                            processor.GetType().Name);

                        await processor.ProcessBatchAsync(notification.BatchId, cancellationToken);
                        
                        logger.LogInformation("State transition completed successfully");
                    }
                }
            }
        }
    }
}