using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace StageSide.Data.Database;

public class BaseDbContext(DbContextOptions options) : DbContext(options)
{
	protected override void ConfigureConventions(ModelConfigurationBuilder builder)
	{
		// Disable relationship-related conventions
		builder.Conventions.Remove<RelationshipDiscoveryConvention>();
		builder.Conventions.Remove<ForeignKeyIndexConvention>();
		builder.Conventions.Remove<NavigationEagerLoadingConvention>();
		builder.Conventions.Remove<ManyToManyJoinEntityTypeConvention>();
		
		base.ConfigureConventions(builder);
	}
}
