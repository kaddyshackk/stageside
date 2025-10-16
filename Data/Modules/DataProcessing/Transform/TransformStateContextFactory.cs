using ComedyPull.Data.Modules.Common;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing.Transform
{
    /// <summary>
    /// Design-time factory for TransformStateContext to support EF migrations.
    /// </summary>
    public class TransformStateContextFactory : BaseDesignTimeDbContextFactory<TransformStateContext>
    {
        /// <summary>
        /// Creates a new instance of TransformStateContext with the provided options.
        /// </summary>
        /// <param name="options">The configured DbContext options.</param>
        /// <returns>A new TransformStateContext instance.</returns>
        protected override TransformStateContext CreateContext(DbContextOptions<TransformStateContext> options)
        {
            return new TransformStateContext(options);
        }
    }
}