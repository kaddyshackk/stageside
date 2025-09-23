using ComedyPull.Application.Features.DataSync.Jobs;
using ComedyPull.Application.Features.DataSync.Punchup;
using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace ComedyPull.Api.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobController(
        ISchedulerFactory schedulerFactory,
        ILogger<JobController> logger
    )
        : ControllerBase
    {
        [HttpPost("punchup")]
        public async Task<IActionResult> TriggerPunchupScrape()
        {
            try
            {
                var scheduler = await schedulerFactory.GetScheduler();
                
                if (!await scheduler.CheckExists(PunchupScrapeJob.Key))
                    return NotFound($"Job {PunchupScrapeJob.Key} not found.");
                
                await scheduler.TriggerJob(PunchupScrapeJob.Key);
                
                return Ok(new { message = "Punchup scrape job completed successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Punchup scrape job failed");
                return StatusCode(500, new { error = "Scrape job failed", details = ex.Message });
            }
        }
    }
}