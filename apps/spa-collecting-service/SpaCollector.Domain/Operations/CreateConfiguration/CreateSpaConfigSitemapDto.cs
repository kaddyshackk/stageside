using System.ComponentModel.DataAnnotations;

namespace StageSide.SpaCollector.Domain.Operations.CreateConfiguration;

public class CreateSpaConfigSitemapDto
{
    [Required]
    public required Guid SkuId { get; set; }
    [Required]
    public required string Url { get; set; }
    public string? RegexFilter { get; set; }
}