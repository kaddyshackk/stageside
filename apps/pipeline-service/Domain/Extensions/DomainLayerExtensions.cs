using ComedyPull.Domain.Interfaces.Processing;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Services;
using ComedyPull.Domain.Sources.Punchup;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Domain.Extensions
{
    public static class DomainLayerExtensions
    {
        public static void AddDomainLayer(this IServiceCollection services)
        {
            // Services
            services.AddSingleton<ActService>();
            services.AddSingleton<VenueService>();
            services.AddSingleton<EventService>();
            services.AddSingleton<JobService>();
            
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