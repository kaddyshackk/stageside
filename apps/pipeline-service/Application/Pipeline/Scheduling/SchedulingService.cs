using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Pipeline.Scheduling
{
    public class SchedulingService(
        IServiceScopeFactory scopeFactory,
        ISitemapLoader sitemapLoader,
        IQueueClient queueClient,
        IQueueHealthMonitor queueHealthMonitor,
        IBackPressureManager backPressureManager,
        IOptions<SchedulingOptions> options,
        ILogger<SchedulingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (await backPressureManager.ShouldApplyBackPressureAsync(Queues.Collection))
                    {
                        logger.LogWarning("Collection queue overloaded, delaying for another interval.");
                        continue;
                    }

                    using var scope = scopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<ISchedulingRepository>();

                    var nextJob = await repository.GetNextJobAsync(stoppingToken);
                    if (nextJob == null)
                    {
                        continue;
                    }

                    logger.LogInformation("Processing job [{Jobname}]", nextJob.Name);

                    await ProcessJobAsync(repository, nextJob, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in scheduling service execution");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(options.Value.DelayIntervalSeconds), stoppingToken);
                }
            }
        }

        private async Task ProcessJobAsync(ISchedulingRepository repository, Job job, CancellationToken stoppingToken)
        {
            var execution = await repository.CreateJobExecutionAsync(job.Id, stoppingToken);
            var sitemaps = await repository.GetJobSitemapsAsync(job.Id, stoppingToken);
            try
            {
                var urls = await GetSitemapUrlsAsync(sitemaps);
                var pipelineContexts = urls.Select(url => new PipelineContext
                {
                    JobExecutionId = execution.Id,
                    Source = job.Source,
                    Sku = job.Sku,
                    Metadata = new PipelineMetadata { CollectionUrl = url }
                }).ToList();

                await queueClient.EnqueueBatchAsync(Queues.Collection, pipelineContexts);
                await queueHealthMonitor.RecordEnqueueAsync(Queues.Collection, pipelineContexts.Count);

                await repository.UpdateJobExecutionAsScheduledAsync(execution.Id, stoppingToken);
                logger.LogInformation("Scheduled job {JobId} with {UrlCount} URLs", job.Id, pipelineContexts.Count);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Job execution {ExecutionId} failed: {Message}", execution.Id, e.Message);
                await repository.UpdateJobExecutionAsFailedAsync(execution.Id, e.Message, stoppingToken);
                await queueHealthMonitor.RecordErrorAsync(Queues.Collection);
            }
        }

        private async Task<List<string>> GetSitemapUrlsAsync(ICollection<JobSitemap> sitemaps)
        {
            var allUrls = new List<string>();
            if (sitemaps.Count != 0)
            {
                foreach (var sitemap in sitemaps.OrderBy(s => s.ProcessingOrder))
                {
                    var urls = await sitemapLoader.LoadSitemapAsync(sitemap.SitemapUrl);
                    allUrls.AddRange(urls);
                }
            }
            else
            {
                throw new NotImplementedException("Collection for Sku's without sitemaps is not supported.");
            }
            return allUrls;
        } 
    }
}