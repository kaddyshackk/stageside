using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Configurations
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
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            builder.Property(e => e.CreatedBy)
                .IsRequired();
            
            builder.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            builder.Property(e => e.UpdatedBy)
                .IsRequired();
        }
    }
}