using System.ComponentModel.DataAnnotations;
using StageSide.SpaCollector.Domain.Operations.CreateConfiguration;

namespace StageSide.SpaCollector.Service.Operations.CreateSpaConfig
{
    public record CreateSpaConfigRequest
    {
        [Required]
        public required string SkuId { get; init; }
        [Required]
        public required int MaxConcurrency { get; init; }
        [Required]
        public string? UserAgent { get; init; }

        public ICollection<CreateSpaConfigSitemapDto> Sitemaps { get; set; } = []!;
    }
}
