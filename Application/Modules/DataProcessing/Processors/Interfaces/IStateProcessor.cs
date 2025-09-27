namespace ComedyPull.Application.Modules.DataProcessing.Processors.Interfaces
{
    public interface IStateProcessor<out TState> where TState : Enum
    {
        public TState FromState { get; }
        public TState ToState { get; }
        public Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken);
    }
}