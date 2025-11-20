using StageSide.Domain.Models;

namespace StageSide.SpaCollector.Domain.Collection.Models;

public record CollectionConfig : AuditableEntity
{
    public Guid Id { get; set; }
    public required Guid SkuId { get; set; }
    public string? UserAgent { get; set; }
    public int MaxConcurrency { get; set; }
    public ICollection<Sitemap> Sitemaps { get; set; } = [];
}