using System.ComponentModel.DataAnnotations;

namespace StageSide.Scheduler.Service.Operations.CreateSource
{
    public record CreateSourceRequest
    {
        [Required]
        public required string Name { get; init; }
        
        [Required]
        public required string Website { get; init; }
    }
}