using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using StageSide.Collection.WebBrowser;
using StageSide.SpaCollector.Data.Collection;
using StageSide.SpaCollector.Data.Database;
using StageSide.SpaCollector.Data.PlaywrightAdapter;
using StageSide.SpaCollector.Data.Utils;
using StageSide.SpaCollector.Domain.Collection.Interfaces;
using StageSide.SpaCollector.Domain.Database;

namespace StageSide.SpaCollector.Data.Extensions;

public static class DataExtensions
{
    public static void AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // Context
        services.AddDbContextFactory<SpaCollectingDbContext>((_, options) =>
        {
            DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration, "ComedyDb");
        });
        services.AddScoped<ISpaCollectingDbContextSession, SpaCollectingDbContextSession>();
        
        // Sitemap Services
        services.AddSingleton<ISitemapLoader, SitemapLoader>();
        
        // Web Browser Services
        services.AddSingleton<IPlaywright>(_ => Playwright.CreateAsync().Result);
        services.AddSingleton<IWebBrowser, PlaywrightWebBrowserAdapter>();
    }
}