using ComedyPull.Application.Features.Ingest.Interfaces;
using System.Text.RegularExpressions;
using ComedyPull.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Features.Ingest.Punchup
{
    public partial class PunchupScrapeJob(
        ISitemapLoader sitemapLoader,
        [FromKeyedServices(DataSource.Punchup)] IScraper scraper,
        IServiceProvider serviceProvider,
        ILogger<PunchupScrapeJob> logger
    )
    {
        public async Task ExecuteAsync()
        {
            logger.LogInformation("DataSync - Job started - {JobName}", nameof(PunchupScrapeJob));
            const string sitemapUrl = "https://www.punchup.live/sitemap.xml";
            try
            {
                var urls = await sitemapLoader.LoadSitemapAsync(sitemapUrl);
                
                // Filter Urls
                var regex = TicketsPageUrlRegex();
                var matched = urls.Where(url => regex.IsMatch(url)).ToList();
                
                // Perform Job
                await scraper.InitializeAsync();
                if (matched.Any())
                {
                    await scraper.RunAsync(matched.Take(3), () => serviceProvider.GetRequiredService<PunchupTicketsPageProcessor>());
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataSync - Job failed - {JobName}", nameof(PunchupScrapeJob));
            }
            finally
            {
                scraper.Dispose();
                logger.LogInformation("DataSync - Job finished - {JobName}", nameof(PunchupScrapeJob));
            }
        }

        [GeneratedRegex(@"^https?:\/\/(?:www\.)?punchup\.live\/([^\/]+)\/tickets(?:\/)?$")]
        private static partial Regex TicketsPageUrlRegex();

        public static Regex GetTicketsPageUrlRegexForTesting() => TicketsPageUrlRegex();
    }
}
