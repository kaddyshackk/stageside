namespace ComedyPull.Domain.Jobs.Interfaces
{
    public interface IJobSitemapRepository
    {
        public Task<JobSitemap> CreateJobSitemapAsync(JobSitemap sitemap, CancellationToken stoppingToken);
        public Task<ICollection<JobSitemap>> ReadJobSitemapsForJobAsync(Guid jobId, CancellationToken stoppingToken);
    }
}