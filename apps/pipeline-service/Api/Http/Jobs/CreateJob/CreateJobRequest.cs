using System.ComponentModel.DataAnnotations;

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
    }
}