using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Interfaces.Repository
{
    public interface IActRepository
    {
        public Task<IEnumerable<Act>> GetActsBySlugAsync(IEnumerable<string> slugs);
        public Task BulkCreateActsAsync(IEnumerable<Act> acts);
        public Task BulkUpdateActsAsync(IEnumerable<Act> acts);
    }
}