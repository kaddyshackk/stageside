using ComedyPull.Data.Modules.Common;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataSync
{
    /// <summary>
    /// Design-time factory for DataSyncContext to support EF migrations.
    /// </summary>
    public class DataSyncContextFactory : BaseDesignTimeDbContextFactory<DataSyncContext>
    {
        /// <summary>
        /// Creates a new instance of DataSyncContext with the provided options.
        /// </summary>
        /// <param name="options">The configured DbContext options.</param>
        /// <returns>A new DataSyncContext instance.</returns>
        protected override DataSyncContext CreateContext(DbContextOptions<DataSyncContext> options)
        {
            return new DataSyncContext(options);
        }
    }
}