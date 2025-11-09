using ComedyPull.Domain.Jobs;
using ComedyPull.Domain.Jobs.Services;
using ComedyPull.Domain.Jobs.Services.Interfaces;
using ComedyPull.Domain.Pipeline;
using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace ComedyPull.Application.Pipeline.Scheduling
{
    public class SchedulingService(
        IServiceScopeFactory scopeFactory,
        IQueueClient queueClient,
        IBackPressureManager backPressureManager,
        IOptions<SchedulingOptions> options,
        ILogger<SchedulingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (LogContext.PushProperty("ServiceName", nameof(SchedulingService)))
            {
                logger.LogInformation("Started {ServiceName}", nameof(SchedulingService));
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (await backPressureManager.ShouldApplyBackPressureAsync(Queues.Collection))
                        {
                            logger.LogWarning("Collection queue overloaded, delaying for another interval.");
                            continue;
                        }

                        await DispatchNextJobAsync(stoppingToken);
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
                logger.LogInformation("Stopped {ServiceName}", nameof(SchedulingService));
            }
        }

        private async Task DispatchNextJobAsync(CancellationToken ct)
        {
            using var scope = scopeFactory.CreateScope();
            var jobService = scope.ServiceProvider.GetRequiredService<JobAggregateService>();
            var sitemapLoader = scope.ServiceProvider.GetRequiredService<ISitemapLoader>();

            var job = await jobService.GetNextJobForExecutionAsync(ct);
            if (job == null)
            {
                return;
            }
            var sitemaps = job.Sitemaps.Where(j => j.IsActive).ToList();
            if (sitemaps.Count == 0)
            {
                logger.LogError("Failed to dispatch next job. No sitemaps available.");
                return;
            }
            
            var execution = await jobService.CreateExecutionAsync(job.Id, ct);
            try
            {
                var urls = await sitemapLoader.LoadManySitemapsAsync(sitemaps);
                var entries = urls.Select(u => new PipelineContext
                {
                    JobExecutionId = execution.Id,
                    Source = job.Source,
                    Sku = job.Sku,
                    Metadata = new PipelineMetadata { CollectionUrl = u }
                }).ToList();
            
                await queueClient.EnqueueBatchAsync(Queues.Collection, entries);
                await jobService.UpdateExecutionStatusAsync(execution.Id, ExecutionStatus.Executed, ct);
                logger.LogInformation("Job execution {ExecutionId} for job {JobId} with {UrlCount} URLs.", execution.Id, job.Id, entries.Count);
            }
            catch (Exception ex)
            {
                await jobService.UpdateExecutionStatusAsync(execution.Id, ExecutionStatus.Failed, ct);
                throw;
            }
        }
    }
}