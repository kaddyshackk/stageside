namespace StageSide.SpaCollector.Service.Operations.CreateSpaConfig;

public class CreateSpaConfigSitemapResponse
{
    public required Guid Id { get; set; }
    public required Guid SpaConfigId { get; set; }
    public required string Url { get; set; }
    public string? RegexFilter { get; set; }
}
