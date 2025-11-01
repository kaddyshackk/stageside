namespace ComedyPull.Domain.Jobs.Interfaces
{
    public interface IJobSitemapRepository
    {
        public Task<ICollection<JobSitemap>> ReadJobSitemapsForJobAsync(Guid jobId, CancellationToken stoppingToken);
    }
}