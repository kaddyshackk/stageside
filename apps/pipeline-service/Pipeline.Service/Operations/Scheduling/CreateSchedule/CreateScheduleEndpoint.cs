using StageSide.Pipeline.Domain.Scheduling;
using Microsoft.AspNetCore.Http.HttpResults;

namespace StageSide.Pipeline.Service.Operations.Scheduling.CreateSchedule
{
    public class CreateScheduleEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/schedules", HandleAsync)
                .WithOpenApi()
                .WithName("CreateSchedule")
                .WithTags("Schedule")
                .WithSummary("Create a new schedule");
        }
        
        private static async Task<Results<Created<CreateScheduleResponse>, BadRequest<string>>> HandleAsync(CreateScheduleRequest request, SchedulingService scheduling, CancellationToken stoppingToken)
        {
            var command = new CreateScheduleRequestMapper().MapToCommand(request);
            var schedule = await scheduling.CreateScheduleAsync(command, stoppingToken);
            if (schedule == null) return TypedResults.BadRequest("Failed to create schedule");
            var response = new CreateScheduleResponseMapper().MapToResponse(schedule);
            return TypedResults.Created($"/schedules/{response.Id}", response!);
        }
    }
}