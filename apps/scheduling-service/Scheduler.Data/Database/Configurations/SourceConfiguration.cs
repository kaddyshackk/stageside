using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Data.Database.Configurations;

public class SourceConfiguration : BaseEntityConfiguration<Source>
{
    public override void Configure(EntityTypeBuilder<Source> builder)
    {
        base.Configure(builder);

        builder.ToTable("Sources");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();
    }
}