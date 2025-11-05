namespace ComedyPull.Domain.Jobs.Interfaces
{
    public interface IJobRepository
    {
        public Task<bool> ReadJobExistsByIdAsync(Guid id, CancellationToken stoppingToken);
        public Task<Job?> ReadJobByIdAsync(Guid jobId, CancellationToken stoppingToken);
        public Task<Job?> ReadNextJobAsync(CancellationToken stoppingToken);
        public Task<Job> CreateJobAsync(Job job, CancellationToken stoppingToken);
        public Task<Job> UpdateJobAsync(Job job, CancellationToken stoppingToken);
    }
}