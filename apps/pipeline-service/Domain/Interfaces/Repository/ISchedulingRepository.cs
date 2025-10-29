using ComedyPull.Domain.Models.Pipeline;

namespace ComedyPull.Domain.Interfaces.Repository
{
    /// <summary>
    /// Manages data access operations for scheduling. Specifically manages <see cref="Job"/>, <see cref="JobExecution"/>, and <see cref="JobSitemap"/> entities.
    /// </summary>
    public interface ISchedulingRepository
    {
        /// <summary>
        /// Retrieves jobs that are due for immediate execution.
        /// </summary>
        /// <param name="stoppingToken">A cancellation token to stop the request.</param>
        /// <returns>A collection of jobs that are due for execution, or an empty collection if there are none.</returns>
        public Task<ICollection<Job>> GetJobsDueForExecutionAsync(CancellationToken stoppingToken);
        
        /// <summary>
        /// Retrieves sitemaps for a specified <see cref="Job"/>.
        /// </summary>
        /// <param name="jobId">ID of the job to fetch sitemaps for.</param>
        /// <param name="stoppingToken">A cancellation token to stop the request.</param>
        /// <returns>A collection of sitemaps, or an empty collection if there are none.</returns>
        public Task<ICollection<JobSitemap>> GetJobSitemapsAsync(Guid jobId, CancellationToken stoppingToken);
        
        /// <summary>
        /// Creates a new <see cref="JobExecution"/> for the specified <see cref="Job"/>.
        /// </summary>
        /// <param name="jobId">ID of the <see cref="Job"/> to execute.</param>
        /// <param name="stoppingToken">A cancellation token to stop the request.</param>
        /// <returns>The created <see cref="JobExecution"/>.</returns>
        public Task<JobExecution> CreateJobExecutionAsync(Guid jobId, CancellationToken stoppingToken);
        
        /// <summary>
        /// Updates the <see cref="JobExecutionStatus"/> of a <see cref="JobExecution"/> to Scheduled, also sets
        /// the ScheduledAt timestamp.
        /// </summary>
        /// <param name="executionId">ID of the <see cref="JobExecution"/> to update.</param>
        /// <param name="stoppingToken">A cancellation token to stop the request.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        public Task<bool> UpdateJobExecutionAsScheduledAsync(Guid executionId, CancellationToken stoppingToken);
        
        /// <summary>
        /// Updates the <see cref="JobExecutionStatus"/> of a <see cref="JobExecution"/> to Failed, also sets
        /// the CompletedAt timestamp as well as the error message.
        /// </summary>
        /// <param name="executionId">ID of the <see cref="JobExecution"/> to update.</param>
        /// <param name="message">Relevant error message for <see cref="JobExecution"/> failure.</param>
        /// <param name="stoppingToken">A cancellation token to stop the request.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        public Task<bool> UpdateJobExecutionAsFailedAsync(Guid executionId, string message, CancellationToken stoppingToken);
    }
}