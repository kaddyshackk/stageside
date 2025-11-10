using ComedyPull.Domain.Models;
using ComedyPull.Domain.Pipeline;
using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Scheduling;
using ComedyPull.Domain.Sources.Punchup;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Domain.Extensions
{
    public static class DomainExtensions
    {
        public static void AddDomainLayer(this IServiceCollection services)
        {
            // Services
            services.AddScoped<SchedulingService>();
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