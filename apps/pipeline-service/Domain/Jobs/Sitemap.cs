using System.Text.RegularExpressions;
using ComedyPull.Domain.Core.Shared;

namespace ComedyPull.Domain.Jobs
{
    public record Sitemap : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Guid JobId { get; set; }
        public required string Url { get; set; }
        public string? RegexFilter { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Job Job { get; set; } = null!;
    }
}