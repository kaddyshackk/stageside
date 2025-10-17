using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.Common
{
    /// <summary>
    /// Design-time factory for SchemaContext to support EF migrations.
    /// </summary>
    public class SchemaContextFactory : BaseDesignTimeDbContextFactory<SchemaContext>
    {
        /// <summary>
        /// Creates a new instance of SchemaContext with the provided options.
        /// </summary>
        /// <param name="options">The configured DbContext options.</param>
        /// <returns>A new SchemaContext instance.</returns>
        protected override SchemaContext CreateContext(DbContextOptions<SchemaContext> options)
        {
            return new SchemaContext(options);
        }
    }
}