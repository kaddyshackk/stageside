using Microsoft.AspNetCore.Http.HttpResults;
using StageSide.Scheduler.Domain.Operations.GetSource;
using StageSide.Scheduler.Domain.Scheduling;

namespace StageSide.Scheduler.Service.Operations.GetSource
{
    public class GetSourceEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/sources/{sourceId}", HandleAsync)
                .WithOpenApi()
                .WithName("GetSource")
                .WithTags("Source")
                .WithSummary("Get an existing source");
        }
        
        private static async Task<Results<Ok<GetSourceResponse>, BadRequest<string>>> HandleAsync(string sourceId, SchedulingService scheduling, CancellationToken ct)
        {
            var query = new GetSourceQuery { Id = Guid.Parse(sourceId) };
            var source = await scheduling.GetSourceByIdAsync(query, ct);
            if (source == null) return TypedResults.BadRequest("Failed to get source by id.");
            var response = new GetSourceResponseMapper().MapToResponse(source);
            return TypedResults.Ok(response);
        }
    }
}