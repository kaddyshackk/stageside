using StageSide.Domain.Models;

namespace StageSide.SpaCollector.Domain.Collection.Models
{
    public record Sitemap : AuditableEntity
    {
        public Guid Id { get; set; }
        public required Guid ScheduleId { get; set; }
        public required string Url { get; set; }
        public string? RegexFilter { get; set; }
        public bool IsActive { get; set; } = true;
    }
}