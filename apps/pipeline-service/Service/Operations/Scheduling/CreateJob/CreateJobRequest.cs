using System.ComponentModel.DataAnnotations;
using ComedyPull.Domain.Operations;

namespace ComedyPull.Service.Operations.Scheduling.CreateJob
{
    public record CreateJobRequest
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