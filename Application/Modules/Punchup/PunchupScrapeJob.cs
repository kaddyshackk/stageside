using System.Text.RegularExpressions;
using ComedyPull.Application.Modules.DataProcessing;
using ComedyPull.Application.Modules.DataSync.Services.Interfaces;
using ComedyPull.Application.Modules.Punchup.Collectors;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Models.Processing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ComedyPull.Application.Modules.Punchup
{
    public partial class PunchupScrapeJob(
        ISitemapLoader sitemapLoader,
        [FromKeyedServices(DataSourceKeys.Punchup)]
        IScraper scraper,
        IServiceProvider serviceProvider,
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
                    await scraper.RunAsync(matched,
                        () => serviceProvider.GetRequiredService<PunchupTicketsPageCollector>());
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