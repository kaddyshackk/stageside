using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Interfaces.Repository
{
    /// <summary>
    /// Manages data access operations for <see cref="Venue"/> entities.
    /// </summary>
    public interface IVenueRepository
    {
        /// <summary>
        /// Retrieves venues matching the specified slug identifiers with change tracking. This method should be used
        /// for update operations.
        /// </summary>
        /// <param name="slugs">The slug identifiers to match. Must not be null.</param>
        /// <param name="stoppingToken">A cancellation token to stop the request.</param>
        /// <returns>A collection of matching venues, or an empty collection if none found.</returns>
        public Task<ICollection<Venue>> GetVenuesBySlugAsync(IEnumerable<string> slugs, CancellationToken stoppingToken);
        
        /// <summary>
        /// Creates multiple venues in a single batch operation.
        /// </summary>
        /// <param name="venues">The venues to create. Must not contain null elements.</param>
        /// <param name="stoppingToken">A cancellation token to stop the request.</param>
        public Task BulkCreateVenuesAsync(IEnumerable<Venue> venues, CancellationToken stoppingToken);
        
        /// <summary>
        /// Saves all changes made to currently tracked entities. Only applies to changes made during a single lifetime
        /// of this repository. This method be used to implement efficient bulk updates.
        /// </summary>
        /// <param name="stoppingToken">A cancellation token to stop the request.</param>
        public Task SaveChangesAsync(CancellationToken stoppingToken);
    }
}