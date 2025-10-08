using ComedyPull.Domain.Models.Processing;

namespace ComedyPull.Application.Modules.DataProcessing.Processors.Interfaces
{
    public interface ISubProcessor<TKey> where TKey : struct
    {
        TKey? Key { get; }
        ProcessingState FromState { get; }
        ProcessingState ToState { get; }
        Task ProcessAsync(IEnumerable<SourceRecord> records, CancellationToken cancellationToken);
    }
}