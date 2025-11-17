using Microsoft.AspNetCore.Http.HttpResults;
using StageSide.Scheduler.Domain.Scheduling;

namespace StageSide.Scheduler.Service.Scheduling.Operations.CreateSchedule
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
        
        private static async Task<Results<Created<CreateScheduleResponse>, BadRequest<string>>> HandleAsync(CreateScheduleRequest request, SchedulingService scheduling, CancellationToken ct)
        {
            var command = new CreateScheduleRequestMapper().MapToCommand(request);
            var schedule = await scheduling.CreateScheduleAsync(command, ct);
            if (schedule == null) return TypedResults.BadRequest("Failed to create schedule");
            var response = new CreateScheduleResponseMapper().MapToResponse(schedule);
            return TypedResults.Created($"/schedules/{response.Id}", response!);
        }
    }
}