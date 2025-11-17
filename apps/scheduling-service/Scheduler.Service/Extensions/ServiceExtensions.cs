using StageSide.Scheduler.Service.Dispatching;
using StageSide.Scheduler.Service.Scheduling;

namespace StageSide.Scheduler.Service.Extensions;

public static class ServiceExtensions
{
    public static void AddServiceLayer(this IServiceCollection services)
    {
        // Dispatching services
        services.AddHostedService<DispatchingService>();
        services.AddSingleton<ExecutionService>();
    }
}