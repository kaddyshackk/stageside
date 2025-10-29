using ComedyPull.Data.Models;
using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Contexts.ComedyDb.Configurations
{
    /// <summary>
    /// Configuration class for the ComedianEvents table.
    /// </summary>
    public class EventActConfiguration : BaseEntityConfiguration<EventAct>
    {
        /// <summary>
        /// Configures the ComedianEvents table.
        /// </summary>
        /// <param name="builder">The EntityTypeBuilder instance.</param>
        public new void Configure(EntityTypeBuilder<EventAct> builder)
        {
            base.Configure(builder);
            
            builder.ToTable("EventActs");
            
            builder.HasKey(ce => new { ComedianId = ce.ActId, ce.EventId });
            
            builder.HasOne(ce => ce.Act)
                .WithMany(c => c.EventActs)
                .HasForeignKey(ce => ce.ActId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(ce => ce.Event)
                .WithMany(e => e.ComedianEvents)
                .HasForeignKey(ce => ce.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}