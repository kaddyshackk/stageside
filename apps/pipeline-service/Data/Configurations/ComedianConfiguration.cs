using ComedyPull.Domain.Modules.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Configurations
{
    /// <summary>
    /// Configuration class for the Comedian table.
    /// </summary>
    public class ComedianConfiguration : TraceableEntityConfiguration<Comedian>
    {
        /// <summary>
        /// Configures the Comedian table.
        /// </summary>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        public new void Configure(EntityTypeBuilder<Comedian> builder)
        {
            base.Configure(builder);

            builder.ToTable("Comedians");
            
            builder.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Property(c => c.Slug)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(c => c.Bio)
                .HasMaxLength(2000)
                .IsRequired();
            
            builder.HasIndex(c => c.Slug).IsUnique();
            builder.HasIndex(c => c.Name);
            builder.HasIndex(c => c.Source);
        }
    }
}