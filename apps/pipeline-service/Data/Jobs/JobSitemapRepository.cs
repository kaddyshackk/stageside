using ComedyPull.Data.Contexts.PipelineDb;
using ComedyPull.Domain.Jobs;
using ComedyPull.Domain.Jobs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Jobs
{
    public class JobSitemapRepository(PipelineDbContext context) : IJobSitemapRepository
    {
        public async Task<ICollection<JobSitemap>> ReadJobSitemapsForJobAsync(Guid jobId, CancellationToken stoppingToken)
        {
            return await context.JobSitemaps
                .Where(s => s.JobId == jobId)
                .ToListAsync(stoppingToken);
        }
    }
}