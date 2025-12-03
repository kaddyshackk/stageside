using Microsoft.AspNetCore.Http.HttpResults;
using StageSide.SpaCollector.Domain.Configuration;

namespace StageSide.SpaCollector.Service.Operations.CreateSpaConfig
{
    public class CreateSpaConfigEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/configurations", HandleAsync)
                .WithOpenApi()
                .WithName("CreateConfiguration")
                .WithTags("Configuration")
                .WithSummary("Create a new configuration");
        }
        
        private static async Task<Results<Created<CreateSpaConfigResponse>, BadRequest<string>>> HandleAsync(CreateSpaConfigRequest request, ConfigurationService service, CancellationToken ct)
        {
            var command = new CreateSpaConfigRequestMapper().MapToCommand(request);
            var config = await service.CreateCollectionConfigAsync(command, ct);
            if (config == null) return TypedResults.BadRequest("Failed to create collection config.");
            var response = new CreateSpaConfigResponseMapper().MapToResponse(config);
            return TypedResults.Created($"/schedules/{response.Id}", response!);
        }
    }
}
