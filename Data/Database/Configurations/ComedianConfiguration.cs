using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Database.Configurations
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
            
            // Properties
            
            builder.Property(c => c.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Property(c => c.Slug)
                .HasColumnName("slug")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(c => c.Bio)
                .HasColumnName("name")
                .HasMaxLength(2000)
                .IsRequired();
            
            // Relationships

            builder.HasMany(c => c.Events)
                .WithMany(e => e.Comedians)
                .UsingEntity<ComedianEvent>();
            
            // Indices
            
            builder.HasIndex(c => c.Slug).IsUnique();
            builder.HasIndex(c => c.Name);
            builder.HasIndex(c => c.Source);
        }
    }
}