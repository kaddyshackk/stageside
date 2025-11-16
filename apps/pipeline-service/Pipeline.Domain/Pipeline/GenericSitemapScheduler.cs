using Microsoft.Extensions.Logging;
using StageSide.Pipeline.Domain.Exceptions;
using StageSide.Pipeline.Domain.Pipeline.Models;
using StageSide.Pipeline.Domain.PipelineAdapter;
using StageSide.Pipeline.Domain.Scheduling.Interfaces;
using StageSide.Pipeline.Domain.Scheduling.Models;

namespace StageSide.Pipeline.Domain.Pipeline
{
    public class GenericSitemapScheduler(ISitemapLoader sitemapLoader, ILogger<GenericSitemapScheduler> logger) : IScheduler
    {
        public async Task<ICollection<PipelineContext>> ScheduleAsync(Schedule schedule, Job job, CancellationToken ct)
        {
            var sitemaps = schedule.Sitemaps.Where(j => j.IsActive).ToList();
            if (sitemaps.Count == 0)
            {
                throw new InvalidScheduleStateException("Expected schedule to contain active sitemaps, but none were found.");
            }
            
            var urls = await sitemapLoader.LoadManySitemapsAsync(sitemaps);
            return urls.Select(u => new PipelineContext
            {
                JobId = job.Id,
                Source = schedule.Source,
                Sku = schedule.Sku,
                Metadata = new PipelineMetadata { CollectionUrl = u }
            }).ToList();
        }
    }
}