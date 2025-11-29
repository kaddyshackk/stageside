using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Domain.Models;

namespace StageSide.Data.Configuration
{
    /// <summary>
    /// Configuration class for the BaseEntity fields of a table.
    /// </summary>
    public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : AuditableEntity
    {
        /// <summary>
        /// Configures the BaseEntity fields of a table.
        /// </summary>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(e => e.CreatedAt)
	            .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            builder.Property(e => e.CreatedBy)
	            .HasColumnName("created_by")
                .IsRequired();
            
            builder.Property(e => e.UpdatedAt)
	            .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            builder.Property(e => e.UpdatedBy)
	            .HasColumnName("updated_by")
                .IsRequired();
        }
    }
}
