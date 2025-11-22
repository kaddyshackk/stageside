namespace StageSide.SpaCollector.Service.Operations.CreateSpaConfig
{
    public class CreateSpaConfigResponse
    {
        public required string Id { get; set; }
        public required string SkuId { get; set; }
        public string? UserAgent { get; set; }
        public int MaxConcurrency { get; set; }
        public ICollection<CreateSpaConfigSitemapResponse> Sitemaps { get; set; }
    }
}