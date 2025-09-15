using ComedyPull.Application.Features.DataSync.Interfaces;
using System.Text.RegularExpressions;
using ComedyPull.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Features.DataSync.Punchup
{
    public partial class PunchupScrapeJob
    {
        private readonly ISitemapLoader _sitemapLoader;
        private readonly IScraper _scraper;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PunchupScrapeJob> _logger;

        public PunchupScrapeJob(
            ISitemapLoader sitemapLoader,
            [FromKeyedServices(DataSourceKeys.Punchup)] IScraper scraper,
            IServiceProvider serviceProvider,
            ILogger<PunchupScrapeJob> logger)
        {
            _sitemapLoader = sitemapLoader;
            _scraper = scraper;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("DataSync - Job started - {JobName}", nameof(PunchupScrapeJob));
            const string sitemapUrl = "https://www.punchup.live/sitemap.xml";
            try
            {
                var urls = await _sitemapLoader.LoadSitemapAsync(sitemapUrl);

                // Filter Urls
                var regex = TicketsPageUrlRegex();
                var matched = urls.Where(url => regex.IsMatch(url)).ToList();

                // Perform Job
                await _scraper.InitializeAsync();
                if (matched.Any())
                {
                    await _scraper.RunAsync(matched,
                        () => _serviceProvider.GetRequiredService<PunchupTicketsPageProcessor>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DataSync - Job failed - {JobName}", nameof(PunchupScrapeJob));
            }
            finally
            {
                _scraper.Dispose();
                _logger.LogInformation("DataSync - Job finished - {JobName}", nameof(PunchupScrapeJob));
            }
        }

        [GeneratedRegex(@"^https?:\/\/(?:www\.)?punchup\.live\/([^\/]+)\/tickets(?:\/)?$")]
        private static partial Regex TicketsPageUrlRegex();

        public static Regex GetTicketsPageUrlRegexForTesting() => TicketsPageUrlRegex();
    }
}