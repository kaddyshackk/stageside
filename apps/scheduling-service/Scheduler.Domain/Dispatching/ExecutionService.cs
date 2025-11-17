using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StageSide.Scheduler.Domain.ContextSession;
using StageSide.Scheduler.Domain.Exceptions;
using StageSide.Scheduler.Domain.Models;
using StageSide.Scheduler.Domain.Scheduling;

namespace StageSide.Scheduler.Domain.Dispatching
{
    public class ExecutionService(ISchedulingContextSession session, ILogger<ExecutionService> logger)
    {
        public async Task<Schedule?> GetNextSchedule(CancellationToken ct)
        {
            return await session.Schedules.Query()
                .AsNoTracking()
                .Include(s => s.Sitemaps)
                .Where(s => s.IsActive && s.NextExecution <= DateTimeOffset.UtcNow)
                .OrderBy(s => s.NextExecution)
                .FirstOrDefaultAsync(ct);
        }
        
        public async Task<Job> CreateJobAsync(Guid scheduleId, CancellationToken ct)
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
                    throw new InvalidScheduleStateException("Schedule is more than one minute in the future.");
                }

                var now = DateTimeOffset.UtcNow;
                var job = new Job
                {
                    ScheduleId = schedule.Id,
                    Status = JobStatus.Created,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                };
                await session.Jobs.AddAsync(job, ct);
                
                if (schedule.CronExpression == null)
                {
                    schedule.IsActive = false;
                }
                else
                {
                    var nextExecution = CronCalculationService.CalculateNextOccurence(schedule.CronExpression);
                    if (!nextExecution.HasValue)
                    {
                        logger.LogError("Failed to calculate next occurence for schedule {ScheduleId} and job {JobId}", schedule.Id, job.Id);
                        throw new InvalidJobStateException($"Failed to calculate next occurence for schedule {schedule.Id}. Cron expression is invalid.");
                    }
                    schedule.NextExecution = nextExecution.Value;
                }
                
                schedule.LastExecuted = now;
                schedule.UpdatedAt = now;
                schedule.UpdatedBy = "System";
                session.Schedules.Update(schedule);
                
                await session.SaveChangesAsync(ct);
                await session.CommitTransactionAsync(ct);
                
                return job;
            }
            catch (Exception ex)
            {
                await session.RollbackTransactionAsync(ct);
                session.Dispose();
                logger.LogError(ex, "Failed to create job.");
                throw;
            }
        }
    }
}