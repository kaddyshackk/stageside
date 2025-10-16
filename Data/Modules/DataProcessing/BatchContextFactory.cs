using ComedyPull.Data.Modules.Common;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing
{
    /// <summary>
    /// Design-time factory for BatchContext to support EF migrations.
    /// </summary>
    public class BatchContextFactory : BaseDesignTimeDbContextFactory<BatchContext>
    {
        /// <summary>
        /// Creates a new instance of BatchContext with the provided options.
        /// </summary>
        /// <param name="options">The configured DbContext options.</param>
        /// <returns>A new BatchContext instance.</returns>
        protected override BatchContext CreateContext(DbContextOptions<BatchContext> options)
        {
            return new BatchContext(options);
        }
    }
}
