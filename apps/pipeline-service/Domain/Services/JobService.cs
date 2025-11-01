using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace ComedyPull.Domain.Services
{
    public class JobService(
        IServiceScopeFactory scopeFactory,
        ISitemapLoader sitemapLoader,
        IQueueClient queueClient,
        ILogger<JobService> logger)
    {
        public async Task DispatchNextJobAsync(CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            var job = await repository.GetNextJobAsync(stoppingToken);
            if (job == null) return;

            using (LogContext.PushProperty("JobId", job.Id))
            {
                var execution = await repository.CreateJobExecutionAsync(job.Id, stoppingToken);
                using (LogContext.PushProperty("ExecutionId", execution.Id))
                {
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
                        await repository.UpdateJobExecutionAsScheduledAsync(execution.Id, stoppingToken);
                        logger.LogInformation("Job execution {ExecutionId} for job {JobId} with {UrlCount} URLs.", execution.Id, job.Id, pipelineContexts.Count);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Job execution {ExecutionId} failed: {Message}", execution.Id, e.Message);
                        await repository.UpdateJobExecutionAsFailedAsync(execution.Id, e.Message, stoppingToken);
                    }
                }
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