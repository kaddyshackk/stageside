using ComedyPull.Domain.Modules.Common;
using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces
{
    /// <summary>
    /// Repository for Complete state operations - handles Silver to Gold entity transformations.
    /// </summary>
    public interface ICompleteStateRepository
    {
        // Silver record operations
        /// <summary>
        /// Retrieves all Silver records associated with a specific batch.
        /// </summary>
        /// <param name="batchId">The unique identifier of the batch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of Silver records.</returns>
        Task<IEnumerable<SilverRecord>> GetSilverRecordsByBatchId(string batchId, CancellationToken cancellationToken);

        /// <summary>
        /// Updates existing Silver records in the database.
        /// </summary>
        /// <param name="records">The Silver records to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task UpdateSilverRecordsAsync(IEnumerable<SilverRecord> records, CancellationToken cancellationToken);

        // Comedian (Gold) operations
        /// <summary>
        /// Retrieves comedians by their slugs.
        /// </summary>
        /// <param name="slugs">The slugs to search for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of matching comedians.</returns>
        Task<IEnumerable<Comedian>> GetComediansBySlugAsync(IEnumerable<string> slugs, CancellationToken cancellationToken);

        /// <summary>
        /// Adds new comedians to the database.
        /// </summary>
        /// <param name="comedians">The comedians to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task AddComediansAsync(IEnumerable<Comedian> comedians, CancellationToken cancellationToken);

        /// <summary>
        /// Updates existing comedians in the database.
        /// </summary>
        /// <param name="comedians">The comedians to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task UpdateComediansAsync(IEnumerable<Comedian> comedians, CancellationToken cancellationToken);

        // Venue (Gold) operations
        /// <summary>
        /// Retrieves venues by their slugs.
        /// </summary>
        /// <param name="slugs">The slugs to search for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of matching venues.</returns>
        Task<IEnumerable<Venue>> GetVenuesBySlugAsync(IEnumerable<string> slugs, CancellationToken cancellationToken);

        /// <summary>
        /// Adds new venues to the database.
        /// </summary>
        /// <param name="venues">The venues to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task AddVenuesAsync(IEnumerable<Venue> venues, CancellationToken cancellationToken);

        /// <summary>
        /// Updates existing venues in the database.
        /// </summary>
        /// <param name="venues">The venues to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task UpdateVenuesAsync(IEnumerable<Venue> venues, CancellationToken cancellationToken);

        // Event (Gold) operations
        /// <summary>
        /// Retrieves events by their slugs.
        /// </summary>
        /// <param name="slugs">The slugs to search for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of matching events.</returns>
        Task<IEnumerable<Event>> GetEventsBySlugAsync(IEnumerable<string> slugs, CancellationToken cancellationToken);

        /// <summary>
        /// Adds new events to the database.
        /// </summary>
        /// <param name="events">The events to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task AddEventsAsync(IEnumerable<Event> events, CancellationToken cancellationToken);

        /// <summary>
        /// Updates existing events in the database.
        /// </summary>
        /// <param name="events">The events to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task UpdateEventsAsync(IEnumerable<Event> events, CancellationToken cancellationToken);

        // ComedianEvent (many-to-many) operations
        /// <summary>
        /// Adds comedian-event relationships to the database.
        /// </summary>
        /// <param name="comedianEvents">The comedian-event relationships to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task AddComedianEventsAsync(IEnumerable<ComedianEvent> comedianEvents, CancellationToken cancellationToken);
    }
}