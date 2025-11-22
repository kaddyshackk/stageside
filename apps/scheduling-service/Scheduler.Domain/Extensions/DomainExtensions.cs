using Microsoft.Extensions.DependencyInjection;
using StageSide.Scheduler.Domain.Dispatching;
using StageSide.Scheduler.Domain.Scheduling;

namespace StageSide.Scheduler.Domain.Extensions;

public static class DomainExtensions
{
    public static void AddDomainLayer(this IServiceCollection services)
    {
        // Dispatching
        services.AddScoped<ExecutionService>();
        
        // Scheduling
        services.AddScoped<SchedulingService>();
    }
}