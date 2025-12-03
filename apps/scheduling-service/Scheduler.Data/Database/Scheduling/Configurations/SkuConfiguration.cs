using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Data.Database.Scheduling.Configurations;

public class SkuConfiguration : BaseEntityConfiguration<Sku>
{
    public override void Configure(EntityTypeBuilder<Sku> builder)
    {
        base.Configure(builder);

        builder.ToTable("skus");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Id)
	        .HasColumnName("id");
        
        builder.Property(s => s.SourceId)
	        .HasColumnName("source_id")
            .IsRequired();

        builder.Property(s => s.Name)
	        .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(s => s.Type)
	        .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(x => x.IsActive)
	        .HasColumnName("is_active")
	        .HasDefaultValue(true)
	        .IsRequired();
        
        builder.HasOne(x => x.Source)
	        .WithMany(x => x.Skus)
	        .HasForeignKey(x => x.SourceId)
	        .OnDelete(DeleteBehavior.Cascade);
    }
}
