using Microsoft.Extensions.Logging;
using StageSide.Pipeline.Interfaces;
using StageSide.Pipeline.Models;
using StageSide.SpaCollector.Domain.Collection.Interfaces;

namespace StageSide.SpaCollector.Domain.Collection
{
    public class GenericSitemapScheduler(ISitemapLoader sitemapLoader, ILogger<GenericSitemapScheduler> logger) : IScheduler
    {
        public async Task<ICollection<PipelineContext>> ScheduleAsync()
        {
            // var sitemaps = schedule.Sitemaps.Where(j => j.IsActive).ToList();
            // if (sitemaps.Count == 0)
            // {
            //     throw new InvalidScheduleStateException("Expected schedule to contain active sitemaps, but none were found.");
            // }
            //
            // var urls = await sitemapLoader.LoadManySitemapsAsync(sitemaps);
            // return urls.Select(u => new PipelineContext
            // {
            //     JobId = job.Id,
            //     Source = schedule.Source,
            //     Sku = schedule.Sku,
            //     Metadata = new PipelineMetadata { CollectionUrl = u }
            // }).ToList();
            return [];
        }
    }
}