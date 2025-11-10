namespace StageSide.Pipeline.Domain.Operations
{
    public class SitemapDto
    {
        public required string Url { get; init; }
        public string? RegexFilter  { get; init; }
    }
}