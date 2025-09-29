using System.Text.RegularExpressions;
using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.DataSync.Services.Interfaces;
using ComedyPull.Application.Modules.Punchup.Factories;
using ComedyPull.Domain.Models.Processing;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ComedyPull.Application.Modules.Punchup
{
    public partial class PunchupScrapeJob(
        ISitemapLoader sitemapLoader,
        IPlaywrightScraperFactory scraperFactory,
        IPunchupTicketsPageCollectorFactory collectorFactory,
        IMediator mediator,
        ILogger<PunchupScrapeJob> logger)
        : IJob
    {
        public static JobKey Key { get; } = new (nameof(PunchupScrapeJob));

        public async Task Execute(IJobExecutionContext context)
        {
            var batchId = Guid.NewGuid();
            logger.LogInformation("DataSync - Job started: {JobName} - {BatchId}", nameof(PunchupScrapeJob), batchId);
            const string sitemapUrl = "https://www.punchup.live/sitemap.xml";

            var scraper = scraperFactory.CreateScraper();
            try
            {
                var urls = await sitemapLoader.LoadSitemapAsync(sitemapUrl);

                // Filter Urls
                var regex = TicketsPageUrlRegex();
                var matched = urls.Where(url => regex.IsMatch(url)).ToList();

                // Limit records for testing if specified
                var maxRecords = context.MergedJobDataMap.GetInt("maxRecords");
                if (maxRecords > 0)
                {
                    matched = matched.Take(maxRecords).ToList();
                    logger.LogInformation("Limited to {MaxRecords} records for testing", maxRecords);
                }

                // Perform Job
                await scraper.InitializeAsync();
                if (matched.Count != 0)
                {
                    await scraper.RunAsync(matched, collectorFactory.CreateCollector);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataSync - Job failed - {JobName}", nameof(PunchupScrapeJob));
            }
            finally
            {
                scraper.Dispose();

                await mediator.Publish(new StateCompletedEvent(batchId, ProcessingState.Ingested));

                logger.LogInformation("DataSync - Job finished - {JobName}", nameof(PunchupScrapeJob));
            }
        }

        [GeneratedRegex(@"^https?:\/\/(?:www\.)?punchup\.live\/([^\/]+)\/tickets(?:\/)?$")]
        private static partial Regex TicketsPageUrlRegex();

        public static Regex GetTicketsPageUrlRegexForTesting() => TicketsPageUrlRegex();
    }
}