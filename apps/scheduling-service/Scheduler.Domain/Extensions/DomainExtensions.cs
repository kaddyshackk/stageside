using Microsoft.Extensions.DependencyInjection;
using StageSide.Scheduler.Domain.Scheduling;

namespace StageSide.Scheduler.Domain.Extensions;

public static class DomainExtensions
{
    public static void AddDomainLayer(this IServiceCollection services)
    {
        services.AddScoped<SchedulingService>();
    }
}