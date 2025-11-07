using ComedyPull.Domain.Core.Events;
using ComedyPull.Domain.Core.Shared;

namespace ComedyPull.Domain.Core.Acts
{
    public record Act : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the comedian id.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();
        
        /// <summary>
        /// Gets or sets the act name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the slug identifier.
        /// </summary>
        public required string Slug { get; set; }

        /// <summary>
        /// Gets or sets the act bio.
        /// </summary>
        public required string Bio { get; set; }

        /// <summary>
        /// Navigation property to ComedianEvent relationship table.
        /// </summary>
        public virtual ICollection<EventAct> EventActs { get; init; } = new List<EventAct>();
    }
}
