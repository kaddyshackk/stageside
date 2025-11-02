using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Interfaces.Repository
{
    /// <summary>
    /// Manages data access operations for <see cref="EventAct"/> entities.
    /// </summary>
    public interface IEventActRepository
    {
        /// <summary>
        /// Creates multiple <see cref="EventAct"/> relationship entities in a single operation.
        /// </summary>
        /// <param name="eventActs">The EventActs to create.</param>
        public Task BulkCreateEventActsAsync(IEnumerable<EventAct> eventActs, CancellationToken stoppingToken);
    }
}