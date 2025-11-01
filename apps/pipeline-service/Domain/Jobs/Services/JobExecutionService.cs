using ComedyPull.Domain.Exceptions;
using ComedyPull.Domain.Jobs.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Domain.Jobs.Services
{
    public class JobExecutionService(IServiceScopeFactory scopeFactory, ILogger<JobExecutionService> logger)
    {
        public async Task<JobExecution> CreateJobExecutionAsync(Guid jobId, CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var executionRepository = scope.ServiceProvider.GetRequiredService<IJobExecutionRepository>();
            
            var job = await jobRepository.ReadJobByIdAsync(jobId, stoppingToken);
            if (job == null)
            {
                throw new NullJobException($"Could not find job with id {jobId}");
            }

            // Do not execute jobs before they are scheduled.
            if (job.NextExecution > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                throw new InvalidJobStateException("Job to execute is more than one minute in the future.");
            }

            var execution = await executionRepository.CreateJobExecutionAsync(new JobExecution
            {
                JobId = job.Id,
                
            }, stoppingToken);
            
            job.LastExecuted = DateTimeOffset.UtcNow;

            if (job.CronExpression == null)
            {
                job.IsActive = false;
            }
            else
            {
                job.NextExecution = CronCalculationService.CalculateNextOccurence(job.CronExpression);
                if (job.NextExecution == null)
                {
                    logger.LogWarning("Failed to calculate next occurence for job {JobId} and execution {ExecutionId}", job.Id, execution.Id);
                }
            }
            
            await jobRepository.UpdateJobAsync(job, stoppingToken);
            return execution;
        }

        public async Task UpdateJobExecutionStatusAsync(Guid executionId, JobExecutionStatus status,
            CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var executionRepository = scope.ServiceProvider.GetRequiredService<IJobExecutionRepository>();
            
            var execution = await executionRepository.ReadJobExecutionByIdAsync(executionId, stoppingToken);
            if (execution == null)
            {
                throw new NullJobException($"Could not find job execution with id {executionId}");
            }
            
            execution.Status = status;
            if (status == JobExecutionStatus.Completed) execution.CompletedAt = DateTimeOffset.UtcNow;
            
            await executionRepository.UpdateJobExecutionAsync(execution, stoppingToken);
        }
    }
}