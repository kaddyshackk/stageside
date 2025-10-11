using ComedyPull.Application.Modules.DataSync.Interfaces;
using ComedyPull.Application.Modules.DataSync.Options;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Modules.DataSync
{
    public class PlaywrightScraperFactory : IPlaywrightScraperFactory
    {
        private readonly IOptions<DataSyncOptions> _options;

        public PlaywrightScraperFactory(IOptions<DataSyncOptions> options)
        {
            _options = options;
        }

        public IScraper CreateScraper()
        {
            return new PlaywrightScraper(
                concurrency: _options.Value.PunchupCollection.Concurrency
            );
        }
    }
}