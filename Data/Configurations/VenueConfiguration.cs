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
            
            builder.ToTable("Venues");
            
            builder.Property(v => v.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Property(v => v.Slug)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasMany(v => v.Events)
                .WithOne(e => e.Venue)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(v => v.Slug).IsUnique();
        }
    }
}