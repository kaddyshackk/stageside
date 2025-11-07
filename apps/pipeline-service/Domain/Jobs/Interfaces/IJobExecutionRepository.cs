namespace ComedyPull.Domain.Jobs.Interfaces
{
    public interface IJobExecutionRepository
    {
        public Task<Execution?> ReadJobExecutionByIdAsync(Guid executionId, CancellationToken stoppingToken);
        public Task<Execution> CreateJobExecutionAsync(Execution execution, CancellationToken stoppingToken);
        public Task<Execution> UpdateJobExecutionAsync(Execution execution, CancellationToken stoppingToken);
    }
}