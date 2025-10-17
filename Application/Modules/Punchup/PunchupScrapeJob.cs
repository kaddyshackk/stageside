using System.Text.Json;
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
            var parameters = GetJobParameters(context);
            
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
                var matched = urls.Where(url => TicketsPageUrlRegex().IsMatch(url)).ToList();

                // Limit records for testing if specified
                if (parameters.MaxRecords is > 0)
                {
                    matched = matched.Take(parameters.MaxRecords.Value).ToList();
                    logger.LogInformation("Limited to {MaxRecords} records for testing", parameters.MaxRecords.Value);
                }

                await scraper.InitializeAsync();
                if (matched.Count != 0)
                {
                    await scraper.RunAsync(matched, () => collectorFactory.CreateCollector(batch.Id));
                }

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

        private static PunchupJobParameters GetJobParameters(IJobExecutionContext context)
        {
            if (!context.MergedJobDataMap.TryGetValue("parameters", out var parametersObj) ||
                parametersObj?.ToString() is not { } parametersJson) return new PunchupJobParameters();
            try
            {
                return JsonSerializer.Deserialize<PunchupJobParameters>(parametersJson) ?? new PunchupJobParameters();
            }
            catch (JsonException)
            {
                // Fall back to empty parameters if deserialization fails
            }

            return new PunchupJobParameters();
        }

        [GeneratedRegex(@"^https?:\/\/(?:www\.)?punchup\.live\/([^\/]+)\/tickets(?:\/)?$")]
        private static partial Regex TicketsPageUrlRegex();

        public static Regex GetTicketsPageUrlRegexForTesting() => TicketsPageUrlRegex();
    }
}