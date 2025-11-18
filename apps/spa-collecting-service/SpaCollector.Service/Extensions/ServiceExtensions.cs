using StageSide.Collection.WebBrowser;
using StageSide.Collection.WebBrowser.Params;
using StageSide.SpaCollector.Domain.WebBrowser;
using StageSide.SpaCollector.Domain.WebBrowser.Options;

namespace StageSide.SpaCollector.Service.Extensions;

public static class ServiceExtensions
{
    public static void AddServiceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // WebBrowser Options
        services.Configure<WebBrowserContextOptions>(configuration.GetSection("WebBrowser:Context"));
        services.Configure<WebBrowserLaunchOptions>(configuration.GetSection("WebBrowser:Launch"));
        services.Configure<WebBrowserResourceOptions>(configuration.GetSection("WebBrowser:Resource"));
        
        // Web Browser
        services.AddScoped<IWebBrowserManager, WebBrowserResourceManager>();
        services.AddScoped<IWebPageSessionProvider, WebPageSessionProvider>();
    }
}