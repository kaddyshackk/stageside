using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataProcessing
{
    public static class ProcessingStateMachine
    {
        private static readonly Dictionary<ProcessingState, ProcessingState> ValidTransitions = new()
        {
            { ProcessingState.Ingested, ProcessingState.Transformed },
            { ProcessingState.Transformed, ProcessingState.Completed },
        };

        public static ProcessingState GetNextState(ProcessingState currentStage)
        {
            if (ValidTransitions.TryGetValue(currentStage, out var nextStage))
                return nextStage;
        
            throw new InvalidOperationException($"No valid transition from state {currentStage}");
        }

        public static bool CanTransition(ProcessingState from, ProcessingState to)
        {
            return ValidTransitions.TryGetValue(from, out var validNext) && validNext == to;
        }
    }
}