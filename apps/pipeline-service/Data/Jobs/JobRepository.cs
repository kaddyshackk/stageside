using ComedyPull.Data.Contexts.PipelineDb;
using ComedyPull.Domain.Jobs;
using ComedyPull.Domain.Jobs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Jobs
{
    public class JobRepository(PipelineDbContext context) : IJobRepository
    {
        public async Task<Job?> ReadJobByIdAsync(Guid jobId, CancellationToken stoppingToken)
        {
            return await context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, stoppingToken);
        }

        public async Task<Job?> ReadNextJobAsync(CancellationToken stoppingToken)
        {
            var now = DateTimeOffset.UtcNow;

            return await context.Jobs
                .Where(j => j.IsActive && j.NextExecution != null && j.NextExecution <= now)
                .OrderBy(j => j.NextExecution)
                .FirstOrDefaultAsync(stoppingToken);
        }

        public async Task<Job> CreateJobAsync(Job job, CancellationToken stoppingToken)
        {
            job.CreatedAt = DateTimeOffset.UtcNow;
            job.CreatedBy = "System";
            job.UpdatedAt = DateTimeOffset.UtcNow;
            job.UpdatedBy = "System";
            await context.AddAsync(job, stoppingToken);
            await context.SaveChangesAsync(stoppingToken);
            return job;
        }

        public async Task<Job> UpdateJobAsync(Job job, CancellationToken stoppingToken)
        {
            job.UpdatedAt = DateTimeOffset.UtcNow;
            job.UpdatedBy = "System";
            context.Update(job);
            await context.SaveChangesAsync(stoppingToken);
            return job;
        }
    }
}