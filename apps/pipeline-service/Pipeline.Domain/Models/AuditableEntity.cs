using StageSide.Pipeline.Domain.Interfaces;

namespace StageSide.Pipeline.Domain.Models
{
    /// <summary>
    /// Defines a base entity.
    /// </summary>
    public record AuditableEntity : IAuditable
    {
        /// <summary>
        /// Gets or sets the time the entity was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        
        /// <summary>
        /// Gets or sets the user who created the entity.
        /// </summary>
        public string? CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the time the entity was updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the user who last updated the entity.
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}