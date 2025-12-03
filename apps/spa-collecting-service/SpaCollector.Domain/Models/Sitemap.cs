using StageSide.Domain.Models;

namespace StageSide.SpaCollector.Domain.Models
{
    public record Sitemap : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Guid SpaConfigId { get; set; }
        public required string Url { get; set; }
        public string? RegexFilter { get; set; }
        public bool IsActive { get; set; } = true;
        public SpaConfig SpaConfig { get; set; } = null!;
    }
}
