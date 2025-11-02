using ComedyPull.Application.Http;
using ComedyPull.Application.Http.Jobs.CreateJob;
using ComedyPull.Domain.Jobs.Operations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ComedyPull.Api.Http.Jobs.CreateJob
{
    public class CreateJobEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/jobs", HandleAsync)
                .WithOpenApi()
                .WithName("CreateJob")
                .WithTags("Jobs")
                .WithSummary("Create a new job");
        }
        
        private static async Task<Results<Created<CreateJobResponse>, BadRequest>> HandleAsync(CreateJobRequest request,
            IHandler<CreateJobCommand, CreateJobResponse> handler, CancellationToken stoppingToken)
        {
            var command = new CreateJobRequestMapper().MapToCommand(request);
            var response = await handler.HandleAsync(command, stoppingToken);
            return TypedResults.Created($"/jobs/{response.Id}", response);
        }
    }
}