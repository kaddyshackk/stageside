using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Data.Database.SchedulingDb.Configurations;

public class SourceConfiguration : BaseEntityConfiguration<Source>
{
    public override void Configure(EntityTypeBuilder<Source> builder)
    {
        base.Configure(builder);

        builder.ToTable("sources");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Id)
	        .HasColumnName("id");
        
        builder.Property(s => s.Name)
	        .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(s => s.Website)
	        .HasColumnName("website")
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(x => x.IsActive)
	        .HasColumnName("is_active")
	        .HasDefaultValue(true)
	        .IsRequired();
    }
}
