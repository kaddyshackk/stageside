using StageSide.Pipeline.Domain.Pipeline;
using StageSide.Pipeline.Domain.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using StageSide.Pipeline.Domain.Models;
using StageSide.Pipeline.Domain.PipelineAdapter;
using StageSide.Pipeline.Domain.Sources.Punchup;
using StageSide.Pipeline.Domain.WebBrowser;
using StageSide.Pipeline.Domain.WebBrowser.Interfaces;

namespace StageSide.Pipeline.Domain.Extensions
{
    public static class DomainExtensions
    {
        public static void AddDomainLayer(this IServiceCollection services)
        {
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