using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Interfaces.Repository
{
    public interface IEventRepository
    {
        public Task<IEnumerable<Event>> GetEventsBySlugAsync(IEnumerable<string> slugs);
        public Task BulkCreateEventsAsync(IEnumerable<Event> events);
        public Task BulkUpdateEventsAsync(IEnumerable<Event> events);
    }
}