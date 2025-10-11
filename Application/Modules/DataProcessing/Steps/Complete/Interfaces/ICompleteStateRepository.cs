using ComedyPull.Domain.Modules.Common;
using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces
{
    public interface ICompleteStateRepository
    {
        // Batch operations
        Task<Batch> GetBatchById(string batchId, CancellationToken cancellationToken);
        Task UpdateBatchStateById(string batchId, ProcessingState state, CancellationToken cancellationToken);
        Task UpdateBatchStatusById(string batchId, ProcessingStatus state, CancellationToken cancellationToken);

        // Silver record operations
        Task<IEnumerable<SilverRecord>> GetSilverRecordsByBatchId(string batchId, CancellationToken cancellationToken);
        Task UpdateSilverRecordsAsync(IEnumerable<SilverRecord> records, CancellationToken cancellationToken);

        // Comedian (Gold) operations
        Task<IEnumerable<Comedian>> GetComediansBySlugAsync(IEnumerable<string> slugs, CancellationToken cancellationToken);
        Task AddComediansAsync(IEnumerable<Comedian> comedians, CancellationToken cancellationToken);
        Task UpdateComediansAsync(IEnumerable<Comedian> comedians, CancellationToken cancellationToken);

        // Venue (Gold) operations
        Task<IEnumerable<Venue>> GetVenuesBySlugAsync(IEnumerable<string> slugs, CancellationToken cancellationToken);
        Task AddVenuesAsync(IEnumerable<Venue> venues, CancellationToken cancellationToken);
        Task UpdateVenuesAsync(IEnumerable<Venue> venues, CancellationToken cancellationToken);

        // Event (Gold) operations
        Task<IEnumerable<Event>> GetEventsBySlugAsync(IEnumerable<string> slugs, CancellationToken cancellationToken);
        Task AddEventsAsync(IEnumerable<Event> events, CancellationToken cancellationToken);
        Task UpdateEventsAsync(IEnumerable<Event> events, CancellationToken cancellationToken);

        // ComedianEvent (many-to-many) operations
        Task AddComedianEventsAsync(IEnumerable<ComedianEvent> comedianEvents, CancellationToken cancellationToken);
    }
}