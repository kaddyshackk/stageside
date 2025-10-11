using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces
{
    public interface ISubProcessor<TKey> where TKey : struct
    {
        TKey? Key { get; }
        ProcessingState FromState { get; }
        ProcessingState ToState { get; }
        Task ProcessAsync(IEnumerable<BronzeRecord> records, CancellationToken cancellationToken);
    }
}