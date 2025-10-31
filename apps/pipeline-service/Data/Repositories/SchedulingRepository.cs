using ComedyPull.Data.Contexts.PipelineDb;
using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Models.Pipeline;
using Cronos;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Repositories
{
    /// inheritdoc
    public class SchedulingRepository(PipelineDbContext context) : ISchedulingRepository
    {
        /// inheritdoc
        public async Task<Job?> GetNextJobAsync(CancellationToken stoppingToken)
        {
            var now = DateTimeOffset.UtcNow;

            var job = await context.Jobs
                .Include(j => j.Executions)
                .FirstOrDefaultAsync(j => j.IsActive, stoppingToken);

            if (job == null) return null;

            var lastExecution = job.Executions
                .Where(e => e.Status is JobExecutionStatus.Completed or JobExecutionStatus.Failed)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefault();

            var lastRunTime = lastExecution?.CreatedAt ?? DateTimeOffset.MinValue;
                
            var cronExpression = CronExpression.Parse(job.CronExpression);
            var nextOccurence = cronExpression.GetNextOccurrence(lastRunTime, TimeZoneInfo.Utc);

            if (nextOccurence.HasValue && nextOccurence.Value <= now)
            {
                return job;
            }

            return null;
        }

        /// inheritdoc
        public async Task<ICollection<JobSitemap>> GetJobSitemapsAsync(Guid jobId, CancellationToken stoppingToken)
        {
            return await context.JobSitemaps
                .Where(s => s.JobId == jobId)
                .ToListAsync(stoppingToken);
        }

        /// inheritdoc
        public async Task<JobExecution> CreateJobExecutionAsync(Guid jobId, CancellationToken stoppingToken)
        {
            var entry = await context.JobExecutions.AddAsync(new JobExecution
            {
                JobId = jobId,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "System"
            }, stoppingToken);
            await context.SaveChangesAsync(stoppingToken);
            return entry.Entity;
        }

        /// inheritdoc
        public async Task<bool> UpdateJobExecutionStatusAsync(Guid executionId, JobExecutionStatus status,
            CancellationToken stoppingToken, string message = null!)
        {
            var affected = await context.JobExecutions
                .Where(j => j.Id == executionId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(j => j.Status, status), stoppingToken);
            return affected > 0;
        }

        /// inheritdoc
        public async Task<bool> UpdateJobExecutionAsScheduledAsync(Guid executionId, CancellationToken stoppingToken)
        {
            var affected = await context.JobExecutions
                .Where(j => j.Id == executionId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(j => j.Status, JobExecutionStatus.Scheduled)
                    .SetProperty(j => j.StartedAt, DateTimeOffset.UtcNow), stoppingToken);
            return affected > 0;
        }
        
        /// inheritdoc
        public async Task<bool> UpdateJobExecutionAsFailedAsync(Guid executionId, string message, CancellationToken stoppingToken)
        {
            var affected = await context.JobExecutions
                .Where(j => j.Id == executionId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(j => j.Status, JobExecutionStatus.Failed)
                    .SetProperty(j => j.CompletedAt, DateTimeOffset.UtcNow)
                    .SetProperty(j => j.ErrorMessage, message), stoppingToken);
            return affected > 0;
        }
    }
}