namespace ComedyPull.Domain.Jobs.Interfaces
{
    public interface IJobSitemapRepository
    {
        public Task<Sitemap> CreateJobSitemapAsync(Sitemap sitemap, CancellationToken stoppingToken);
        public Task<ICollection<Sitemap>> ReadJobSitemapsForJobAsync(Guid jobId, CancellationToken stoppingToken);
    }
}