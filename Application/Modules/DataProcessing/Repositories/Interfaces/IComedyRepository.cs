using ComedyPull.Domain.Models;

namespace ComedyPull.Application.Modules.DataProcessing.Repositories.Interfaces
{
    public interface IComedyRepository
    {
        Task<Comedian?> GetComedianBySlugAsync(string slug, CancellationToken cancellationToken);
        Task<Venue?> GetVenueBySlugAsync(string slug, CancellationToken cancellationToken);
        void AddComedian(Comedian comedian);
        void AddVenue(Venue venue);
        void AddEvent(Event eventEntity);
        void AddComedianEvent(ComedianEvent comedianEvent);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
