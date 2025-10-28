using ComedyPull.Domain.Interfaces;
using ComedyPull.Domain.Interfaces.Processing;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Sources.Punchup;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Domain.Extensions
{
    public static class DomainLayerExtensions
    {
        public static void AddDomainLayer(this IServiceCollection services)
        {
            AddPunchupSource(services);
        }

        private static void AddPunchupSource(IServiceCollection services)
        {
            services.AddKeyedScoped<IDynamicCollector, PunchupTicketsPageCollector>(ContentSku.PunchupTicketsPage);
            services.AddKeyedScoped<ITransformer, PunchupTicketsPageTransformer>(ContentSku.PunchupTicketsPage);
        }
    }
}