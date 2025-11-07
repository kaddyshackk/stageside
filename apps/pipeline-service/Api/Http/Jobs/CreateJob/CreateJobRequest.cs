using System.ComponentModel.DataAnnotations;
using ComedyPull.Domain.Jobs.Operations.CreateJob;

namespace ComedyPull.Api.Http.Jobs.CreateJob
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