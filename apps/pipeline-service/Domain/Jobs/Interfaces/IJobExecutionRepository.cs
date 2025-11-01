namespace ComedyPull.Domain.Jobs.Interfaces
{
    public interface IJobExecutionRepository
    {
        public Task<JobExecution?> ReadJobExecutionByIdAsync(Guid executionId, CancellationToken stoppingToken);
        public Task<JobExecution> CreateJobExecutionAsync(JobExecution execution, CancellationToken stoppingToken);
        public Task<JobExecution> UpdateJobExecutionAsync(JobExecution execution, CancellationToken stoppingToken);
    }
}