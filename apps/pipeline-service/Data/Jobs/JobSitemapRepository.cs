using ComedyPull.Data.Contexts.PipelineDb;
using ComedyPull.Domain.Jobs;
using ComedyPull.Domain.Jobs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Jobs
{
    public class JobSitemapRepository(PipelineDbContext context) : IJobSitemapRepository
    {
        public async Task<JobSitemap> CreateJobSitemapAsync(JobSitemap sitemap, CancellationToken stoppingToken)
        {
            sitemap.CreatedAt = DateTimeOffset.UtcNow;
            sitemap.CreatedBy = "System";
            sitemap.UpdatedAt = DateTimeOffset.UtcNow;
            sitemap.UpdatedBy = "System";
            var created = await context.JobSitemaps.AddAsync(sitemap, stoppingToken);
            await context.SaveChangesAsync(stoppingToken);
            return created.Entity;
        }

        public async Task<ICollection<JobSitemap>> ReadJobSitemapsForJobAsync(Guid jobId, CancellationToken stoppingToken)
        {
            return await context.JobSitemaps
                .Where(s => s.JobId == jobId)
                .ToListAsync(stoppingToken);
        }
    }
}