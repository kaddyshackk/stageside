using Microsoft.Extensions.DependencyInjection;
using StageSide.Processor.Domain.Processing;
using StageSide.Processor.Domain.Transforming;

namespace StageSide.Processor.Domain.Extensions;

public static class DomainExtensions
{
    public static void AddDomainLayer(this IServiceCollection services)
    {
        services.AddScoped<ActService>();
        services.AddScoped<VenueService>();
        services.AddScoped<EventService>();
    }
}