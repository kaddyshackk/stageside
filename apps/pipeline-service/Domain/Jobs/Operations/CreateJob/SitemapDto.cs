namespace ComedyPull.Domain.Jobs.Operations.CreateJob
{
    public class SitemapDto
    {
        public required string Url { get; init; }
        public string? RegexFilter  { get; init; }
    }
}