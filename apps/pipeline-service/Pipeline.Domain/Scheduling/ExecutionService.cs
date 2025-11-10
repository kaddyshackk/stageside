using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StageSide.Pipeline.Domain.Exceptions;
using StageSide.Pipeline.Domain.Pipeline.Models;
using StageSide.Pipeline.Domain.Scheduling.Interfaces;
using StageSide.Pipeline.Domain.Scheduling.Models;

namespace StageSide.Pipeline.Domain.Scheduling
{
    public class ExecutionService(ISchedulingDataSession session, ISitemapLoader sitemapLoader, ILogger<SchedulingService> logger)
    {
        public async Task<ICollection<PipelineContext>> ExecuteNextScheduleAsync(CancellationToken ct)
        {
            var schedule = await GetNextSchedule(ct);
            if (schedule == null)
            {
                return [];
            }
            
            var sitemaps = schedule.Sitemaps.Where(j => j.IsActive).ToList();
            if (sitemaps.Count == 0)
            {
                logger.LogError("Failed to dispatch next schedule. No sitemaps available.");
                return [];
            }
            
            var execution = await CreateExecutionAsync(schedule.Id, ct);
            var urls = await sitemapLoader.LoadManySitemapsAsync(sitemaps);
            return urls.Select(u => new PipelineContext
            {
                ExecutionId = execution.Id,
                Source = schedule.Source,
                Sku = schedule.Sku,
                Metadata = new PipelineMetadata { CollectionUrl = u }
            }).ToList();
        }
        
        private async Task<Schedule?> GetNextSchedule(CancellationToken ct)
        {
            return await session.Schedules.Query()
                .AsNoTracking()
                .Include(s => s.Sitemaps)
                .Where(s => s.IsActive && s.NextExecution <= DateTimeOffset.UtcNow)
                .OrderBy(s => s.NextExecution)
                .FirstOrDefaultAsync(ct);
        }
        
        private async Task<Execution> CreateExecutionAsync(Guid scheduleId, CancellationToken ct)
        {
            try
            {
                await session.BeginTransactionAsync(ct);
                
                var schedule = await session.Schedules.GetByIdAsync(scheduleId, ct);
                if (schedule == null)
                {
                    throw new NullScheduleException($"Could not find schedule with id {scheduleId}");
                }
                if (schedule.NextExecution > DateTimeOffset.UtcNow.AddMinutes(1))
                {
                    throw new InvalidScheduleStateException("Schedule to execute is more than one minute in the future.");
                }

                var now = DateTimeOffset.UtcNow;
                var execution = new Execution
                {
                    ScheduleId = schedule.Id,
                    Status = ExecutionStatus.Created,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                };
                await session.Executions.AddAsync(execution, ct);
                
                if (schedule.CronExpression == null)
                {
                    schedule.IsActive = false;
                }
                else
                {
                    var nextExecution = CronCalculationService.CalculateNextOccurence(schedule.CronExpression);
                    if (!nextExecution.HasValue)
                    {
                        logger.LogError("Failed to calculate next occurence for schedule {ScheduleId} and execution {ExecutionId}", schedule.Id, execution.Id);
                        throw new InvalidExecutionStateException($"Failed to calculate next occurence for schedule {schedule.Id}. Cron expression is invalid.");
                    }
                    schedule.NextExecution = nextExecution.Value;
                }
                
                schedule.LastExecuted = now;
                schedule.UpdatedAt = now;
                schedule.UpdatedBy = "System";
                session.Schedules.Update(schedule);
                
                await session.SaveChangesAsync(ct);
                await session.CommitTransactionAsync(ct);
                
                return execution;
            }
            catch (Exception ex)
            {
                await session.RollbackTransactionAsync(ct);
                session.Dispose();
                logger.LogError(ex, "Failed to create schedule execution.");
                throw;
            }
        }
        
        public async Task<bool> UpdateExecutionStatusAsync(Guid executionId, ExecutionStatus status,
            CancellationToken ct)
        {
            var execution = await session.Executions.GetByIdAsync(executionId, ct);
            if (execution == null)
            {
                throw new NullScheduleException($"Could not find schedule execution with id {executionId}");
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
                throw new InvalidOperationException("Failed to update schedule execution.");
            }
             
            return true;
        }
    }
}