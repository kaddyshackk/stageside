using Microsoft.AspNetCore.Http.HttpResults;
using StageSide.Scheduler.Domain.Operations.CreateSource;
using StageSide.Scheduler.Domain.Scheduling;

namespace StageSide.Scheduler.Service.Operations.CreateSource
{
    public class CreateSourceEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/sources", HandleAsync)
                .WithOpenApi()
                .WithName("CreateSource")
                .WithTags("Source")
                .WithSummary("Create a new source");
        }
        
        private static async Task<Results<Created<CreateSourceResponse>, BadRequest<string>>> HandleAsync(CreateSourceRequest request, SchedulingService scheduling, CancellationToken ct)
        {
            var command = new CreateSourceCommand
            {
                Name = request.Name,
                Website = request.Website,
            };
            var source = await scheduling.CreateSourceAsync(command, ct);
            if (source == null) return TypedResults.BadRequest("Failed to create source");
            var response = new CreateSourceResponseMapper().MapToResponse(source);
            return TypedResults.Created($"/api/sources/{response.Id}", response);
        }
    }
}