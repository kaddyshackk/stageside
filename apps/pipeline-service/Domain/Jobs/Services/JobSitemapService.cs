using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Jobs.Interfaces;

namespace ComedyPull.Domain.Jobs.Services
{
    public class JobSitemapService(ISitemapLoader sitemapLoader, IJobSitemapRepository sitemapRepository)
    {
        public async Task<List<string>> GetSitemapUrlsForJobAsync(Guid jobId, CancellationToken stoppingToken)
        {
            var sitemaps = await sitemapRepository.ReadJobSitemapsForJobAsync(jobId, stoppingToken);
            return await GetSitemapUrlsAsync(sitemaps);
        }
        
        public async Task<List<string>> GetSitemapUrlsAsync(ICollection<JobSitemap> sitemaps)
        {
            var allUrls = new List<string>();
            if (sitemaps.Count != 0)
            {
                foreach (var sitemap in sitemaps.OrderBy(s => s.ProcessingOrder))
                {
                    var urls = await sitemapLoader.LoadSitemapAsync(sitemap.SitemapUrl);
                    allUrls.AddRange(urls);
                }
            }
            else
            {
                throw new NotImplementedException("Collection for Sku's without sitemaps is not supported.");
            }
            return allUrls;
        }
    }
}