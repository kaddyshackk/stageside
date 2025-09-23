using ComedyPull.Domain.Models.Processing;

namespace ComedyPull.Application.Features.DataProcessing.Interfaces
{
    public interface ITransformProcessor
    {
        ProcessingState FromState { get; }
        ProcessingState ToState { get; }
        Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken);
    }
}