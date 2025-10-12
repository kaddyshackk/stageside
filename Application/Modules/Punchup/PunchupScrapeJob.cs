using System.Text.RegularExpressions;
using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Application.Modules.DataSync.Interfaces;
using ComedyPull.Application.Modules.Punchup.Collectors.Interfaces;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Modules.DataProcessing;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ComedyPull.Application.Modules.Punchup
{
    public partial class PunchupScrapeJob(
        ISitemapLoader sitemapLoader,
        IPlaywrightScraperFactory scraperFactory,
        IPunchupTicketsPageCollectorFactory collectorFactory,
        IBatchRepository batchRepository,
        IMediator mediator,
        ILogger<PunchupScrapeJob> logger)
        : IJob
    {
        public static JobKey Key { get; } = new (nameof(PunchupScrapeJob));

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("DataSync - Job started: {JobName}", nameof(PunchupScrapeJob));
            
            try
            {
                await ScrapeTicketsPages(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataSync - Job failed - {JobName}", nameof(PunchupScrapeJob));
            }
            finally
            {
                logger.LogInformation("DataSync - Job finished - {JobName}", nameof(PunchupScrapeJob));
            }
        }

        private async Task ScrapeTicketsPages(IJobExecutionContext context)
        {
            // Create batch for PunchupTicketsPage scraping
            var batch = await batchRepository.CreateBatch(
                DataSource.Punchup, 
                DataSourceType.PunchupTicketsPage, 
                nameof(PunchupScrapeJob),
                context.CancellationToken);

            logger.LogInformation("DataSync - PunchupTicketsPage scraping started - BatchId: {BatchId}", batch.Id);

            const string sitemapUrl = "https://www.punchup.live/sitemap.xml";
            var scraper = scraperFactory.CreateScraper();
            
            try
            {
                var urls = await sitemapLoader.LoadSitemapAsync(sitemapUrl);

                // Filter Urls
                var regex = TicketsPageUrlRegex();
                var matched = urls.Where(url => regex.IsMatch(url)).ToList();

                // Limit records for testing if specified
                var maxRecordsString = context.MergedJobDataMap.GetString("maxRecords");
                if (!string.IsNullOrEmpty(maxRecordsString) && int.TryParse(maxRecordsString, out var maxRecords) && maxRecords > 0)
                {
                    matched = matched.Take(maxRecords).ToList();
                    logger.LogInformation("Limited to {MaxRecords} records for testing", maxRecords);
                }

                // Perform scraping
                await scraper.InitializeAsync();
                if (matched.Count != 0)
                {
                    await scraper.RunAsync(matched, () => collectorFactory.CreateCollector(batch.Id));
                }

                // Mark batch as completed
                await mediator.Publish(new StateCompletedEvent(Guid.Parse(batch.Id), ProcessingState.Ingested));
                logger.LogInformation("DataSync - PunchupTicketsPage scraping completed - BatchId: {BatchId}", batch.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataSync - PunchupTicketsPage scraping failed - BatchId: {BatchId}", batch.Id);
                throw;
            }
            finally
            {
                scraper.Dispose();
            }
        }

        [GeneratedRegex(@"^https?:\/\/(?:www\.)?punchup\.live\/([^\/]+)\/tickets(?:\/)?$")]
        private static partial Regex TicketsPageUrlRegex();

        public static Regex GetTicketsPageUrlRegexForTesting() => TicketsPageUrlRegex();
    }
}