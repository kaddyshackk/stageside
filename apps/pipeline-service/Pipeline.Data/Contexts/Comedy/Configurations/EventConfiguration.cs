using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.Domain.Models;

namespace StageSide.Pipeline.Data.Contexts.Comedy.Configurations
{
    /// <summary>
    /// Configuration class for the Events table.
    /// </summary>
    public class EventConfiguration : BaseEntityConfiguration<Event>
    {
        /// <summary>
        /// Configures the Events table.
        /// </summary>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        public override void Configure(EntityTypeBuilder<Event> builder)
        {
            base.Configure(builder);

            builder.ToTable("Events");
            
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Title)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Slug)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.StartDateTime)
                .IsRequired();

            builder.Property(e => e.EndDateTime);
            
            builder.HasOne(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.Slug).IsUnique();
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.StartDateTime);
            builder.HasIndex(e => e.VenueId);
        }
    }
}