using System.Reflection;
using StageSide.Scheduler.Service.Operations;

namespace StageSide.Scheduler.Service.Extensions
{
    public static class EndpointExtensions
    {
        public static void MapEndpoints(
            this IEndpointRouteBuilder app,
            Assembly assembly)
        {
            var endpointTypes = assembly.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IEndpoint)) 
                            && t is { IsAbstract: false, IsInterface: false });

            foreach (var type in endpointTypes)
            {
                var method = type.GetMethod(nameof(IEndpoint.Map), 
                    BindingFlags.Public | BindingFlags.Static);
            
                method?.Invoke(null, [app]);
            }
        }
    }
}