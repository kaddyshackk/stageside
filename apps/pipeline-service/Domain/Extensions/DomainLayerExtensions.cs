using ComedyPull.Domain.Core.Acts;
using ComedyPull.Domain.Core.Events.Services;
using ComedyPull.Domain.Core.Shared;
using ComedyPull.Domain.Core.Venues;
using ComedyPull.Domain.Jobs.Services;
using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Sources.Punchup;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Domain.Extensions
{
    public static class DomainLayerExtensions
    {
        public static void AddDomainLayer(this IServiceCollection services)
        {
            // Services
            services.AddScoped<JobService>();
            services.AddScoped<JobExecutionService>();
            services.AddScoped<JobSitemapService>();
            services.AddScoped<JobDispatchService>();
            services.AddScoped<ActService>();
            services.AddScoped<VenueService>();
            services.AddScoped<EventService>();
            
            // Sources
            AddPunchupSource(services);
        }

        private static void AddPunchupSource(IServiceCollection services)
        {
            services.AddKeyedScoped<IDynamicCollector, PunchupTicketsPageCollector>(Sku.PunchupTicketsPage);
            services.AddKeyedScoped<ITransformer, PunchupTicketsPageTransformer>(Sku.PunchupTicketsPage);
        }
    }
}