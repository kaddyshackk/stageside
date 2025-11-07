namespace ComedyPull.Domain.Core.Events.Interfaces
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
        /// <param name="stoppingToken">The cancellation token.</param>
        public Task BulkCreateEventActsAsync(IEnumerable<EventAct> eventActs, CancellationToken stoppingToken);
    }
}