using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Interfaces.Repository
{
    public interface IVenueRepository
    {
        public Task<IEnumerable<Venue>> GetVenuesBySlugAsync(IEnumerable<string> slugs);
        public Task BulkCreateVenuesAsync(IEnumerable<Venue> venues);
        public Task BulkUpdateVenuesAsync(IEnumerable<Venue> venues);
    }
}