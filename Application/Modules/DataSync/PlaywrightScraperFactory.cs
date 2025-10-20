using ComedyPull.Application.Modules.DataSync.Interfaces;
using ComedyPull.Application.Modules.DataSync.Options;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Modules.DataSync
{
    public class PlaywrightScraperFactory(IOptions<DataSyncOptions> options) : IPlaywrightScraperFactory
    {
        public IScraper CreateScraper()
        {
            return new PlaywrightScraper(
                concurrency: options.Value.PunchupCollection.Concurrency
            );
        }
    }
}