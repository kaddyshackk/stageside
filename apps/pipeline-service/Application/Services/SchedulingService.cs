using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Services
{
    public class SchedulingService(
        IServiceScopeFactory scopeFactory,
        ISitemapLoader sitemapLoader,
        IQueueClient queueClient,
        ILogger<SchedulingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISchedulingRepository>();
                var dueJobs = await repository.GetJobsDueForExecutionAsync(stoppingToken);
                
                foreach (var job in dueJobs)
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

                        await repository.UpdateJobExecutionAsScheduledAsync(execution.Id, stoppingToken);
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Job execution failed: {Message}", e.Message);
                        await repository.UpdateJobExecutionAsFailedAsync(execution.Id, e.Message, stoppingToken);
                        throw;
                    }
                }
                
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
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