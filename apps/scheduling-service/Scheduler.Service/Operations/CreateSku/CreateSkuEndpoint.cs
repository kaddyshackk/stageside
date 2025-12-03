using Microsoft.AspNetCore.Http.HttpResults;
using StageSide.Scheduler.Domain.Operations.CreateSku;
using StageSide.Scheduler.Domain.Scheduling;

namespace StageSide.Scheduler.Service.Operations.CreateSku
{
    public class CreateSkuEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/sources/{sourceId}/skus", HandleAsync)
                .WithOpenApi()
                .WithName("CreateSku")
                .WithTags("Sku")
                .WithSummary("Create a new sku");
        }
        
        private static async Task<Results<Created<CreateSkuResponse>, BadRequest<string>>> HandleAsync(string sourceId, CreateSkuRequest request, SchedulingService scheduling, CancellationToken ct)
        {
            var command = new CreateSkuCommand
            {
                SourceId = Guid.Parse(sourceId),
                Name = request.Name,
                Type = request.Type,
            };
            var sku = await scheduling.CreateSkuAsync(command, ct);
            if (sku == null) return TypedResults.BadRequest("Failed to create sku");
            var response = new CreateSkuResponseMapper().MapToResponse(sku);
            return TypedResults.Created($"/api/sources/{response.SourceId}/skus/{response.Id}", response);
        }
    }
}
