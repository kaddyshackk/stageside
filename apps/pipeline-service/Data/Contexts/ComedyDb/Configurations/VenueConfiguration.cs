using ComedyPull.Data.Models;
using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Contexts.ComedyDb.Configurations
{
    /// <summary>
    /// Configuration class for the Venues table.
    /// </summary>
    public class VenueConfiguration : BaseEntityConfiguration<Venue>
    {
        /// <summary>
        /// Configures the Venues table.
        /// </summary>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        public new void Configure(EntityTypeBuilder<Venue> builder)
        {
            base.Configure(builder);
            
            builder.ToTable("Venues");
            
            builder.HasKey(x => x.Id);
            
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