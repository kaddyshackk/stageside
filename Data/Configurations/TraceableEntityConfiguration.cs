using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Configurations
{
    /// <summary>
    /// Configuration class for the TraceableEntity fields of a table.
    /// </summary>
    public class TraceableEntityConfiguration<T> : BaseEntityConfiguration<T> where T : TraceableEntity
    {
        /// <summary>
        /// Configures the TraceableEntity fields of a table.
        /// </summary>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        public new void Configure(EntityTypeBuilder<T> builder)
        {
            base.Configure(builder);
            
            builder.Property(t => t.Source)
                .HasMaxLength(255)
                .IsRequired();
            
            builder.Property(t => t.IngestedAt)
                .IsRequired();

            builder.HasIndex(t => t.Source);
        }
    }
}