using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Processing;

namespace ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces
{
    public interface ICompleteStateRepository
    {
        public Task<IEnumerable<SourceRecord>> GetRecordsByBatchAsync(string batchId,
            CancellationToken cancellationToken = default);
        public Task SaveChangesAsync(CancellationToken cancellationToken = default);
        public Task<Comedian?> GetComedianBySlugAsync(string slug, CancellationToken cancellationToken);
        public Task<Venue?> GetVenueBySlugAsync(string slug, CancellationToken cancellationToken);
        public void AddComedian(Comedian comedian);
        public void AddVenue(Venue venue);
        public void AddEvent(Event eventEntity);
        public void AddComedianEvent(ComedianEvent comedianEvent);
    }
}