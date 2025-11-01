using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace ComedyPull.Domain.Jobs.Services
{
    public class JobDispatchService(
        JobService jobService,
        JobExecutionService executionService,
        JobSitemapService sitemapService,
        IQueueClient queueClient,
        ILogger<JobDispatchService> logger)
    {
        public async Task DispatchNextJobAsync(CancellationToken stoppingToken)
        {
            var job = await jobService.GetNextJob(stoppingToken);
            if (job == null)
            {
                return;
            }
            using (LogContext.PushProperty("JobId", job.Id))
            {
                var execution = await executionService.CreateJobExecutionAsync(job.Id, stoppingToken);
                using (LogContext.PushProperty("ExecutionId", execution.Id))
                {
                    try
                    {
                        var urls = await sitemapService.GetSitemapUrlsForJobAsync(job.Id, stoppingToken);
                        var pipelineContexts = urls.Select(url => new PipelineContext
                        {
                            JobExecutionId = execution.Id,
                            Source = job.Source,
                            Sku = job.Sku,
                            Metadata = new PipelineMetadata { CollectionUrl = url }
                        }).ToList();

                        await queueClient.EnqueueBatchAsync(Queues.Collection, pipelineContexts);
                        await executionService.UpdateJobExecutionStatusAsync(execution.Id, JobExecutionStatus.Executed, stoppingToken);
                        logger.LogInformation("Job execution {ExecutionId} for job {JobId} with {UrlCount} URLs.", execution.Id, job.Id, pipelineContexts.Count);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Job execution {ExecutionId} failed: {Message}", execution.Id, e.Message);
                        await executionService.UpdateJobExecutionStatusAsync(execution.Id, JobExecutionStatus.Failed, stoppingToken);
                    }
                }
            }
        }
    }
}