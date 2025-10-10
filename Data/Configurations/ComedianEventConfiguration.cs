using ComedyPull.Domain.Modules.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Configurations
{
    /// <summary>
    /// Configuration class for the ComedianEvents table.
    /// </summary>
    public class ComedianEventConfiguration : IEntityTypeConfiguration<ComedianEvent>
    {
        /// <summary>
        /// Configures the ComedianEvents table.
        /// </summary>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        public void Configure(EntityTypeBuilder<ComedianEvent> builder)
        {
            builder.ToTable("ComedianEvents");
            
            builder.HasKey(ce => new { ce.ComedianId, ce.EventId });
            
            builder.HasOne(ce => ce.Comedian)
                .WithMany(c => c.ComedianEvents)
                .HasForeignKey(ce => ce.ComedianId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(ce => ce.Event)
                .WithMany(e => e.ComedianEvents)
                .HasForeignKey(ce => ce.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}