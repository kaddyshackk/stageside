using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.Punchup;
using ComedyPull.Domain.Models.Processing;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace ComedyPull.Api.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobController(
        ISchedulerFactory schedulerFactory,
        IMediator mediator,
        ILogger<JobController> logger
    )
        : ControllerBase
    {
        [HttpPost("punchup")]
        public async Task<IActionResult> TriggerPunchupScrape([FromQuery] int? maxRecords = null)
        {
            try
            {
                var scheduler = await schedulerFactory.GetScheduler();
                
                if (!await scheduler.CheckExists(PunchupScrapeJob.Key))
                    return NotFound($"Job {PunchupScrapeJob.Key} not found.");

                var jobDataMap = new JobDataMap();
                if (maxRecords.HasValue)
                    jobDataMap.Put("maxRecords", maxRecords.Value);
                
                await scheduler.TriggerJob(PunchupScrapeJob.Key, jobDataMap);
                
                return Ok(new { message = "Punchup scrape job completed successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Punchup scrape job failed");
                return StatusCode(500, new { error = "Scrape job failed", details = ex.Message });
            }
        }

        [HttpPost("replay-stage/{batchId:guid}")]
        public async Task<IActionResult> ReplayStage(Guid batchId, [FromQuery] string stage)
        {
            try
            {
                if (!Enum.TryParse<ProcessingState>(stage, true, out var processingState))
                {
                    return BadRequest(new { error = $"Invalid stage '{stage}'. Valid stages: {string.Join(", ", Enum.GetNames<ProcessingState>())}" });
                }

                await mediator.Publish(new StateCompletedEvent(batchId, processingState));
                
                logger.LogInformation("Replayed stage {Stage} for batch {BatchId}", processingState, batchId);
                return Ok(new { message = $"Successfully triggered replay of stage {processingState} for batch {batchId}" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to replay stage {Stage} for batch {BatchId}", stage, batchId);
                return StatusCode(500, new { error = "Stage replay failed", details = ex.Message });
            }
        }
    }
}