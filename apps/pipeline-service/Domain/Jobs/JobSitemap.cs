using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Jobs
{
    public record JobSitemap : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Guid JobId { get; set; }
        public required string SitemapUrl { get; set; }
        public required int ProcessingOrder { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Job Job { get; set; } = null!;
    }
}