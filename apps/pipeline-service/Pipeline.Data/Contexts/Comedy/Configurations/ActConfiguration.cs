using StageSide.Pipeline.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Domain.Models;

namespace StageSide.Pipeline.Data.Contexts.Comedy.Configurations
{
    /// <summary>
    /// Configuration class for the Act table.
    /// </summary>
    public class ActConfiguration : BaseEntityConfiguration<Act>
    {
        /// <summary>
        /// Configures the Act table.
        /// </summary>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        public override void Configure(EntityTypeBuilder<Act> builder)
        {
            base.Configure(builder);

            builder.ToTable("Acts");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Property(x => x.Slug)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Bio)
                .HasMaxLength(2000)
                .IsRequired();
            
            builder.HasIndex(x => x.Slug).IsUnique();
            builder.HasIndex(x => x.Name);
        }
    }
}