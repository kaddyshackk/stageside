using System.Text.Json;
using ComedyPull.Application.Modules.Punchup;
using Quartz;

namespace ComedyPull.Api.Modules.Public
{
    public static class JobEndpoints
    {
        public static void AddJobEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/jobs").WithTags("Jobs");

            group.MapPost("/punchup", async (ISchedulerFactory factory, int? maxRecords, CancellationToken ct) =>
                {
                    var scheduler = await factory.GetScheduler(ct);
            
                    if (!await scheduler.CheckExists(PunchupScrapeJob.Key, ct))
                        return Results.NotFound($"Job {PunchupScrapeJob.Key} not found");

                    var parameters = new PunchupJobParameters { MaxRecords = maxRecords };
                    var jobDataMap = new JobDataMap();
                    jobDataMap.Put("parameters", JsonSerializer.Serialize(parameters));
            
                    await scheduler.TriggerJob(PunchupScrapeJob.Key, jobDataMap, ct);
            
                    return Results.Ok(new { message = "Scrape job queued successfully" });
                })
                .WithName("TriggerPunchupScrapeJob")
                .WithOpenApi();
        }
    }
}