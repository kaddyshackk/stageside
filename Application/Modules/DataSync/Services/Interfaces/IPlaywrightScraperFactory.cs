using ComedyPull.Application.Modules.DataSync.Services.Interfaces;

namespace ComedyPull.Application.Modules.DataSync.Services.Interfaces
{
    public interface IPlaywrightScraperFactory
    {
        IScraper CreateScraper();
    }
}