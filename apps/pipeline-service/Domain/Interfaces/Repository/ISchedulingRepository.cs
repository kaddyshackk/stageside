using ComedyPull.Domain.Models.Pipeline;

namespace ComedyPull.Domain.Interfaces.Repository
{
    public interface ISchedulingRepository
    {
        public Task<ICollection<Job>> GetJobsDueForExecutionAsync(CancellationToken stoppingToken);
        
        public Task<ICollection<JobSitemap>> GetJobSitemapsAsync(Guid jobId, CancellationToken stoppingToken);
        
        public Task<int> GetRunningExecutionsCountAsync(Guid jobId, CancellationToken stoppingToken);
        
        public Task<JobExecution> CreateJobExecutionAsync(Guid jobId, CancellationToken stoppingToken);
        
        public Task<JobExecution> UpdateJobExecutionAsync(Guid executionId, int urlCount, JobExecutionStatus status, CancellationToken stoppingToken, string message = null!);
        
        public Task<JobExecution> UpdateLastExecutedAsync(Guid jobId, DateTimeOffset lastExecuted, CancellationToken stoppingToken);
    }
}