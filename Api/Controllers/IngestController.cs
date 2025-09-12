using ComedyPull.Application.Features.Ingest.Punchup;
using Microsoft.AspNetCore.Mvc;

namespace ComedyPull.Api.Controllers
{
    [ApiController]
    [Route("api/ingest")]
    public class IngestController(ILogger<IngestController> logger)
        : ControllerBase
    {
        [HttpPost("punchup")]
        public async Task<IActionResult> TriggerPunchupScrape(
            [FromServices] PunchupScrapeJob job
        )
        {
            try
            {
                await job.ExecuteAsync();
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