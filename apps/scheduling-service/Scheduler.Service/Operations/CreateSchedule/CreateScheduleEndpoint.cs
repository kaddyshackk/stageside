using Microsoft.AspNetCore.Http.HttpResults;
using StageSide.Scheduler.Domain.Operations.CreateSchedule;
using StageSide.Scheduler.Domain.Scheduling;

namespace StageSide.Scheduler.Service.Operations.CreateSchedule
{
    public class CreateScheduleEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/schedules/{sourceId}/skus/{skuId}/schedules", HandleAsync)
                .WithOpenApi()
                .WithName("CreateSchedule")
                .WithTags("Schedule")
                .WithSummary("Create a new schedule");
        }
        
        private static async Task<Results<Created<CreateScheduleResponse>, BadRequest<string>>> HandleAsync(string sourceId, string skuId, CreateScheduleRequest request, SchedulingService scheduling, CancellationToken ct)
        {
            var command = new CreateScheduleCommand
            {
                SourceId = Guid.Parse(sourceId),
                SkuId = Guid.Parse(skuId),
                Name = request.Name,
                CronExpression = request.CronExpression
            };
            var schedule = await scheduling.CreateScheduleAsync(command, ct);
            if (schedule == null) return TypedResults.BadRequest("Failed to create schedule");
            var response = new CreateScheduleResponseMapper().MapToResponse(schedule);
            return TypedResults.Created($"/api/sources/{response.SourceId}/skus/{response.SkuId}schedules/{response.Id}", response);
        }
    }
}