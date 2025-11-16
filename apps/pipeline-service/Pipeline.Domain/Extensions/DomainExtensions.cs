using Microsoft.Extensions.Configuration;
using StageSide.Pipeline.Domain.Pipeline;
using StageSide.Pipeline.Domain.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using StageSide.Collection.WebBrowser;
using StageSide.Collection.WebBrowser.Interfaces;
using StageSide.Collection.WebBrowser.Options;
using StageSide.Domain.Models;
using StageSide.Pipeline.Domain.PipelineAdapter;
using StageSide.Pipeline.Domain.Sources.Punchup;

namespace StageSide.Pipeline.Domain.Extensions
{
    public static class DomainExtensions
    {
        public static void AddDomainLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // Web Browser Options
            services.Configure<WebBrowserContextOptions>(configuration.GetSection("WebBrowser:Context"));
            services.Configure<WebBrowserLaunchOptions>(configuration.GetSection("WebBrowser:Launch"));
            services.Configure<WebBrowserResourceOptions>(configuration.GetSection("WebBrowser:Resource"));
            
            // Services
            services.AddScoped<SchedulingService>();
            services.AddScoped<ExecutionService>();
            services.AddScoped<ActService>();
            services.AddScoped<VenueService>();
            services.AddScoped<EventService>();
            
            // Web Browser
            services.AddScoped<IWebBrowserManager, WebBrowserResourceManager>();
            services.AddScoped<IWebPageSessionProvider, WebPageSessionProvider>();
            
            // Punchup
            services.AddScoped<PunchupTicketsPageCollector>();
            services.AddKeyedScoped<IPipelineAdapter, PunchupTicketsPageAdapter>(Sku.PunchupTicketsPage.GetEnumDescription());
        }
    }
}