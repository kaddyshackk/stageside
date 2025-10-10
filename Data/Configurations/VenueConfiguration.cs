using ComedyPull.Domain.Modules.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Configurations
{
    /// <summary>
    /// Configuration class for the Venues table.
    /// </summary>
    public class VenueConfiguration : TraceableEntityConfiguration<Venue>
    {
        /// <summary>
        /// Configures the Venues table.
        /// </summary>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        public new void Configure(EntityTypeBuilder<Venue> builder)
        {
            base.Configure(builder);
            
            // Properties

            builder.Property(v => v.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Property(v => v.Slug)
                .HasMaxLength(50)
                .IsRequired();

            // Relationships

            builder.HasMany(v => v.Events)
                .WithOne(e => e.Venue)
                .HasForeignKey(v => v.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indices

            builder.HasIndex(v => v.Slug).IsUnique();
        }
    }
}