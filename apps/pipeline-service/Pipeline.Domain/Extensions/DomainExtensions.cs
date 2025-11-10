using StageSide.Pipeline.Domain.Models;
using StageSide.Pipeline.Domain.Pipeline;
using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.Scheduling;
using StageSide.Pipeline.Domain.Sources.Punchup;
using Microsoft.Extensions.DependencyInjection;

namespace StageSide.Pipeline.Domain.Extensions
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