using ComedyPull.Domain.Scheduling;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ComedyPull.Api.Http.Jobs.CreateJob
{
    public class CreateJobEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/jobs", HandleAsync)
                .WithOpenApi()
                .WithName("CreateJob")
                .WithTags("Jobs")
                .WithSummary("Create a new job");
        }
        
        private static async Task<Results<Created<CreateJobResponse>, BadRequest<string>>> HandleAsync(CreateJobRequest request, SchedulingService scheduling, CancellationToken stoppingToken)
        {
            var command = new CreateJobRequestMapper().MapToCommand(request);
            var job = await scheduling.CreateJobAsync(command, stoppingToken);
            if (job == null) return TypedResults.BadRequest("Failed to create job");
            var response = new CreateJobResponseMapper().MapToResponse(job);
            return TypedResults.Created($"/jobs/{response.Id}", response!);
        }
    }
}