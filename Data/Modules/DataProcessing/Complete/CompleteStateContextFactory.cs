using ComedyPull.Data.Modules.Common;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing.Complete
{
    /// <summary>
    /// Design-time factory for CompleteStateContext to support EF migrations.
    /// </summary>
    public class CompleteStateContextFactory : BaseDesignTimeDbContextFactory<CompleteStateContext>
    {
        /// <summary>
        /// Creates a new instance of CompleteStateContext with the provided options.
        /// </summary>
        /// <param name="options">The configured DbContext options.</param>
        /// <returns>A new CompleteStateContext instance.</returns>
        protected override CompleteStateContext CreateContext(DbContextOptions<CompleteStateContext> options)
        {
            return new CompleteStateContext(options);
        }
    }
}