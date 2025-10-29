using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Services
{
    public class SchedulingService(
        ISchedulingRepository repository,
        ISitemapLoader sitemapLoader,
        IQueueClient queueClient,
        ILogger<SchedulingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var dueJobs = await repository.GetJobsDueForExecutionAsync(stoppingToken);
                    foreach (var job in dueJobs)
                    {
                        if (await CanExecuteJobAsync(job, stoppingToken))
                        {
                            await ExecuteJobAsync(job, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in scheduling service execution");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task<bool> CanExecuteJobAsync(Job job, CancellationToken stoppingToken)                                                                                   
        {
            var runningExecutions = await repository
                .GetRunningExecutionsCountAsync(job.Id, stoppingToken);
            return runningExecutions < job.MaxConcurrency;
        }

        private async Task ExecuteJobAsync(Job job, CancellationToken stoppingToken)
        {
            var jobExecution = await repository.CreateJobExecutionAsync(job.Id, stoppingToken);

            try
            {
                var urls = await GetUrlsForJobAsync(job, stoppingToken);
                var pipelineContexts = urls.Select(url => new PipelineContext
                {
                    JobExecutionId = jobExecution.Id,
                    Source = job.Source,
                    Sku = job.Sku,
                    Metadata = new PipelineMetadata { CollectionUrl = url }
                }).ToList();

                await queueClient.EnqueueBatchAsync(Queues.Collection, pipelineContexts);

                await repository.UpdateJobExecutionAsync(jobExecution.Id,
                urls.Count, JobExecutionStatus.Running, stoppingToken);

                await repository.UpdateLastExecutedAsync(job.Id, DateTime.UtcNow, stoppingToken);
            }
            catch (Exception ex)
            {
                await repository.UpdateJobExecutionAsync(jobExecution.Id, 0, JobExecutionStatus.Failed, stoppingToken, ex.Message);
                throw;
            }
        }

        private async Task<List<string>> GetUrlsForJobAsync(Job job, CancellationToken stoppingToken)
        {
            var allUrls = new List<string>();

            var sitemaps = await repository.GetJobSitemapsAsync(job.Id, stoppingToken);

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