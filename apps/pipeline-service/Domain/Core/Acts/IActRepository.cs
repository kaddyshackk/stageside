namespace ComedyPull.Domain.Core.Acts
{
    /// <summary>
    /// Manages data access operations for <see cref="Act"/> entities.
    /// </summary>
    public interface IActRepository
    {
        /// <summary>
        /// Retrieves acts matching the specific slug identifiers without tracking changes. This method should be used
        /// for read-only operations.
        /// </summary>
        /// <param name="slugs">The slug identifiers to match. Must not be null.</param>
        /// <returns>A collection of matching acts, or an empty collection if none found.</returns>
        public Task<ICollection<Act>> ReadActsBySlugAsync(IEnumerable<string> slugs, CancellationToken stoppingToken);
        
        /// <summary>
        /// Retrieves acts matching the specified slug identifiers with change tracking. This method should be used
        /// for update operations.
        /// </summary>
        /// <param name="slugs">The slug identifiers to match. Must not be null.</param>
        /// <returns>A collection of matching acts, or an empty collection if none found.</returns>
        public Task<ICollection<Act>> GetActsBySlugAsync(IEnumerable<string> slugs, CancellationToken stoppingToken);
        
        /// <summary>
        /// Creates multiple acts in a single batch operation.
        /// </summary>
        /// <param name="acts">The acts to create. Must not contain null elements.</param>
        public Task BulkCreateActsAsync(IEnumerable<Act> acts, CancellationToken stoppingToken);
        
        /// <summary>
        /// Saves all changes made to currently tracked entities. Only applies to changes made during a single lifetime
        /// of this repository. This method be used to implement efficient bulk updates.
        /// </summary>
        public Task SaveChangesAsync(CancellationToken stoppingToken);
    }
}