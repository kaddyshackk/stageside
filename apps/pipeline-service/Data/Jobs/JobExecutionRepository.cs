using ComedyPull.Data.Contexts.PipelineDb;
using ComedyPull.Domain.Jobs;
using ComedyPull.Domain.Jobs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Jobs
{
    public class JobExecutionRepository(PipelineDbContext context) : IJobExecutionRepository
    {
        public async Task<Execution?> ReadJobExecutionByIdAsync(Guid executionId, CancellationToken stoppingToken)
        {
            return await context.Executions.FirstOrDefaultAsync(e => e.Id == executionId, stoppingToken);
        }

        public async Task<Execution> CreateJobExecutionAsync(Execution execution, CancellationToken stoppingToken)
        {
            execution.CreatedAt = DateTimeOffset.UtcNow;
            execution.CreatedBy = "System";
            execution.UpdatedAt = DateTimeOffset.UtcNow;
            execution.UpdatedBy = "System";
            await context.Executions.AddAsync(execution, stoppingToken);
            await context.SaveChangesAsync(stoppingToken);
            return execution;
        }

        public async Task<Execution> UpdateJobExecutionAsync(Execution execution, CancellationToken stoppingToken)
        { 
            execution.UpdatedAt = DateTimeOffset.UtcNow;
            execution.UpdatedBy = "System";
            context.Update(execution);
            await context.SaveChangesAsync(stoppingToken);
            return execution;
        }
    }
}