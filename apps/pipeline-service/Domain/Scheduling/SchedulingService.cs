using ComedyPull.Domain.Exceptions;
using ComedyPull.Domain.Extensions;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Operations;
using ComedyPull.Domain.Scheduling.Interfaces;
using ComedyPull.Domain.Scheduling.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Domain.Scheduling
{
    public class SchedulingService(ISchedulingDataSession session, ILogger<SchedulingService> logger)
    {
        public async Task<Job?> GetNextJobForExecutionAsync(CancellationToken ct)
        {
            return await session.Jobs.Query()
                .Include(j => j.Sitemaps)
                .Where(j => j.IsActive && j.NextExecution <= DateTimeOffset.UtcNow)
                .OrderBy(j => j.NextExecution)
                .FirstOrDefaultAsync(ct);
        }
        
        public async Task<Job?> CreateJobAsync(CreateJobCommand command, CancellationToken ct)
        {
            try
            {
                var nextOccurence = string.IsNullOrEmpty(command.CronExpression)
                    ? DateTimeOffset.UtcNow
                    : CronCalculationService.CalculateNextOccurence(command.CronExpression);
                if (nextOccurence == null)
                {
                    throw new ArgumentException("Failed to determine next occurence for new job.");
                }

                var source = EnumExtensions.ParseFromDescriptionOrThrow<Source>(command.Source);
                var sku = EnumExtensions.ParseFromDescriptionOrThrow<Sku>(command.Sku);

                await session.BeginTransactionAsync(ct);

                var now = DateTimeOffset.UtcNow;
                var job = new Job
                {
                    Source = source,
                    Sku = sku,
                    Name = command.Name,
                    CronExpression = command.CronExpression,
                    IsActive = true,
                    NextExecution = nextOccurence.Value,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                };
                await session.Jobs.AddAsync(job, ct);

                if (command.Sitemaps is { Count: > 0 })
                {
                    var sitemaps = command.Sitemaps
                        .Select(s => new Sitemap
                        {
                            JobId = job.Id,
                            Url = s.Url,
                            RegexFilter = s.RegexFilter,
                            CreatedAt = now,
                            CreatedBy = "System",
                            UpdatedAt = now,
                            UpdatedBy = "System",
                        })
                        .ToList();
                    await session.Sitemaps.AddRangeAsync(sitemaps, ct);
                }
                
                await session.SaveChangesAsync(ct);
                await session.CommitTransactionAsync(ct);

                return await session.Jobs.Query()
                    .Include(j => j.Sitemaps)
                    .FirstOrDefaultAsync(j => j.Id == job.Id, ct);
            }
            catch (Exception ex)
            {
                await session.RollbackTransactionAsync(ct);
                session.Dispose();
                logger.LogError(ex, "Failed to create job.");
                throw;
            }
        }
        
        public async Task<Execution> CreateExecutionAsync(Guid jobId, CancellationToken ct)
        {
            try
            {
                await session.BeginTransactionAsync(ct);
                
                // Fetch & validate job
                var job = await session.Jobs.GetByIdAsync(jobId, ct);
                if (job == null)
                {
                    throw new NullJobException($"Could not find job with id {jobId}");
                }
                if (job.NextExecution > DateTimeOffset.UtcNow.AddMinutes(1))
                {
                    throw new InvalidJobStateException("Job to execute is more than one minute in the future.");
                }

                // Create execution
                var now = DateTimeOffset.UtcNow;
                var execution = new Execution
                {
                    JobId = job.Id,
                    Status = ExecutionStatus.Created,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                };
                await session.Executions.AddAsync(execution, ct);
                
                // Check if one time job or needs next occurence
                if (job.CronExpression == null)
                {
                    job.IsActive = false;
                }
                else
                {
                    var nextExecution = CronCalculationService.CalculateNextOccurence(job.CronExpression);
                    if (!nextExecution.HasValue)
                    {
                        logger.LogError("Failed to calculate next occurence for job {JobId} and execution {ExecutionId}", job.Id, execution.Id);
                        throw new InvalidJobExecutionStateException($"Failed to calculate next occurence for job {job.Id}. Cron expression is invalid.");
                    }
                    job.NextExecution = nextExecution.Value;
                }
                
                job.LastExecuted = now;
                job.UpdatedAt = now;
                job.UpdatedBy = "System";
                session.Jobs.Update(job);
                
                await session.SaveChangesAsync(ct);
                await session.CommitTransactionAsync(ct);
                
                return execution;
            }
            catch (Exception ex)
            {
                await session.RollbackTransactionAsync(ct);
                session.Dispose();
                logger.LogError(ex, "Failed to create job execution.");
                throw;
            }
        }

        public async Task<bool> UpdateExecutionStatusAsync(Guid executionId, ExecutionStatus status,
            CancellationToken ct)
        {
            var execution = await session.Executions.GetByIdAsync(executionId, ct);
            if (execution == null)
            {
                throw new NullJobException($"Could not find job execution with id {executionId}");
            }
                
            var now = DateTimeOffset.UtcNow;
            execution.Status = status;
            execution.UpdatedAt = now;
            execution.UpdatedBy = "System";
            if (status == ExecutionStatus.Completed)
            {
                execution.CompletedAt = DateTimeOffset.UtcNow;
            }
                
            var affected = await session.SaveChangesAsync(ct);
            if (affected == 0)
            {
                throw new InvalidOperationException("Failed to update job execution.");
            }
             
            return true;
        }
    }
}