namespace ComedyPull.Domain.Interfaces.Model
{
    /// <summary>
    /// Represents an auditable entity with several tracking fields.
    /// </summary>
    public interface IAuditable
    {
        /// <summary>
        /// Gets or sets the time the entity was created.
        /// </summary>
        DateTimeOffset CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the user who created the entity.
        /// </summary>
        string CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the time the entity was updated.
        /// </summary>
        DateTimeOffset UpdatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the user who last updated the entity.
        /// </summary>
        string UpdatedBy { get; set; }
    }
}