using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StageSide.Collection.WebBrowser;
using StageSide.Collection.WebBrowser.Params;
using StageSide.Pipeline.Interfaces;
using StageSide.Punchup.Adapter;
using StageSide.SpaCollector.Domain.Collection;
using StageSide.SpaCollector.Domain.Configuration;
using StageSide.SpaCollector.Domain.WebBrowser;
using StageSide.SpaCollector.Domain.WebBrowser.Options;

namespace StageSide.SpaCollector.Domain.Extensions;

public static class DomainExtensions
{
    public static void AddDomainLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration Services
        services.AddScoped<ConfigurationService>();
        
        // Collection Services
        services.AddTransient<SpaCollectionJob>();
        
        // Web Browser
        services.AddSingleton<IWebBrowserManager, WebBrowserResourceManager>();
        services.AddScoped<IWebPageSessionProvider, WebPageSessionProvider>();
        
        // WebBrowser Options
        services.Configure<WebBrowserContextOptions>(configuration.GetSection("WebBrowser:Context"));
        services.Configure<WebBrowserLaunchOptions>(configuration.GetSection("WebBrowser:Launch"));
        services.Configure<WebBrowserResourceOptions>(configuration.GetSection("WebBrowser:Resource"));

        // Source Adapters
        services.AddSingleton<IPipelineAdapterFactory, PipelineAdapterFactory>();
        services.AddScoped<IPipelineAdapter, PunchupTicketsPageAdapter>();
    }
}