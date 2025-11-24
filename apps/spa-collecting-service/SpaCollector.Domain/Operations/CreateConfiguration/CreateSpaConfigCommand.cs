namespace StageSide.SpaCollector.Domain.Operations.CreateConfiguration;

public class CreateSpaConfigCommand
{
    public required Guid SkuId { get; set; }
    public required int MaxConcurrency { get; set; }
    public string? UserAgent { get; set; }
    public required ICollection<CreateSpaConfigSitemapDto> Sitemaps { get; set; } = [];
}