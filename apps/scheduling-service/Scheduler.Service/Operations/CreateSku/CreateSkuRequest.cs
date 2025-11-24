using System.ComponentModel.DataAnnotations;
using StageSide.Domain.Models;

namespace StageSide.Scheduler.Service.Operations.CreateSku
{
    public record CreateSkuRequest
    {
        [Required]
        public required string Name { get; set; }
        
        [Required]
        public required SkuType Type { get; set; }
    }
}