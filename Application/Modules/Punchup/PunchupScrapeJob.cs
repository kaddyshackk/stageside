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
using Serilog.Context;

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
            using (LogContext.PushProperty("JobName", nameof(PunchupScrapeJob)))
            {
                logger.LogInformation("Job started.");
                try
                {
                    await ScrapeTicketsPages(context);
                    logger.LogInformation("Job finished.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Job failed.");
                }
            }
        }

        private async Task ScrapeTicketsPages(IJobExecutionContext context)
        {
            var parameters = GetJobParameters(context);

            logger.LogDebug("Job Params: {@Parameters}", parameters);
            
            var batch = await batchRepository.CreateBatch(
                DataSource.Punchup, 
                DataSourceType.PunchupTicketsPage, 
                nameof(PunchupScrapeJob),
                context.CancellationToken
            );

            using (LogContext.PushProperty("@Batch", batch))
            using (LogContext.PushProperty("SourceType", batch.SourceType))
            {
                // TODO: Move to database configuration. Support multiple sitemaps.
                const string sitemapUrl = "https://www.punchup.live/sitemap.xml";
                var scraper = scraperFactory.CreateScraper();
            
                try
                {
                    var urls = await sitemapLoader.LoadSitemapAsync(sitemapUrl);
                    var matched = urls.Where(url => TicketsPageUrlRegex().IsMatch(url)).ToList();

                    if (parameters.MaxRecords is > 0)
                    {
                        matched = matched.Take(parameters.MaxRecords.Value).ToList();
                        logger.LogDebug("Limited batch to {MaxRecords} records.", parameters.MaxRecords.Value);
                    }

                    using (LogContext.PushProperty("BatchSize", matched.Count))
                    {
                        await scraper.InitializeAsync();
                        if (matched.Count != 0)
                        {
                            logger.LogInformation("Starting batch scrape job");
                            await scraper.RunAsync(matched, () => collectorFactory.CreateCollector(batch.Id));
                        }

                        await batchRepository.UpdateBatchStateById(batch.Id, ProcessingState.Ingested, context.CancellationToken);

                        await mediator.Publish(new StateCompletedEvent(batch.Id, ProcessingState.Ingested));

                        logger.LogInformation("Batch scraping completed.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Batch scraping failed.");
                    throw;
                }
                finally
                {
                    scraper.Dispose();
                }
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

        // TODO: Move to database configuration.
        [GeneratedRegex(@"^https?:\/\/(?:www\.)?punchup\.live\/([^\/]+)\/tickets(?:\/)?$")]
        private static partial Regex TicketsPageUrlRegex();

        public static Regex GetTicketsPageUrlRegexForTesting() => TicketsPageUrlRegex();
    }
}