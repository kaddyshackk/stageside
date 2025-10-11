using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces
{
    public interface IStateProcessor
    {
        ProcessingState FromState { get; }
        ProcessingState ToState { get; }
        Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken);
    }
}