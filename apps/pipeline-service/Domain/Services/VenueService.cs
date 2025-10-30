using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Domain.Services
{
    public class VenueService(IServiceScopeFactory scopeFactory)
    {
        public async Task<BatchProcessResult<ProcessedVenue, Venue>> ProcessVenuesAsync(IEnumerable<ProcessedVenue> processedVenues, CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IVenueRepository>();

            var venues = processedVenues.ToList();
            var slugs = venues.Select(s => s.Slug).Distinct().Where(s => !string.IsNullOrEmpty(s));
            var existingVenues = await repository.GetVenuesBySlugAsync(slugs!, stoppingToken);
            var existingBySlug = existingVenues.ToDictionary(s => s.Slug, s => s);

            var toCreate = new List<Venue>();
            var toUpdate = new List<Venue>();

            foreach (var current in venues)
            {
                if (existingBySlug.TryGetValue(current.Slug!, out var existing))
                {
                    var hasChanges = false;
                    if (!string.IsNullOrEmpty(current.Name) && existing.Name != current.Name)
                    {
                        existing.Name = current.Name;
                        hasChanges = true;
                    }
                    
                    if (!hasChanges) continue;
                    existing.UpdatedAt = DateTimeOffset.UtcNow;
                    existing.UpdatedBy = "System";
                    toUpdate.Add(existing);
                }
                else
                {
                    toCreate.Add(new Venue
                    {
                        Slug = current.Slug,
                        Name = current.Name,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = "System",
                        UpdatedAt = DateTimeOffset.UtcNow,
                        UpdatedBy = "System"
                    });
                }
            }
            
            if (toCreate.Count != 0)
                await repository.BulkCreateVenuesAsync(toCreate, stoppingToken);
            
            if (toUpdate.Count != 0)
                await repository.SaveChangesAsync(stoppingToken);

            return new BatchProcessResult<ProcessedVenue, Venue>
            {
                Created = toCreate,
                Updated = toUpdate,
                Failed = [],
                ProcessedCount = venues.Count
            };
        }
    }
}