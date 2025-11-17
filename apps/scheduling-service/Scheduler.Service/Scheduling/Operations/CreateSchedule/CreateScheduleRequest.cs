using System.ComponentModel.DataAnnotations;
using StageSide.Scheduler.Domain.Scheduling.Operations.CreateSchedule;

namespace StageSide.Scheduler.Service.Scheduling.Operations.CreateSchedule
{
    public record CreateScheduleRequest
    {
        [Required]
        public required string Source { get; init; }
        
        [Required]
        public required string Sku { get; init; }
        
        [Required]
        public required string Name { get; init; }
        
        public string? CronExpression { get; init; }
        
        public ICollection<SitemapDto>? Sitemaps { get; init; }
    }
}