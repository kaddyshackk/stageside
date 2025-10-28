using ComedyPull.Domain.Interfaces.Model;

namespace ComedyPull.Domain.Models
{
    /// <summary>
    /// Defines a base entity.
    /// </summary>
    public record AuditableEntity : IAuditable
    {
        /// <summary>
        /// Gets or sets the time the entity was created.
        /// </summary>
        public required DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        
        /// <summary>
        /// Gets or sets the user who created the entity.
        /// </summary>
        public required string CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the time the entity was updated.
        /// </summary>
        public required DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the user who last updated the entity.
        /// </summary>
        public required string UpdatedBy { get; set; }
    }
}