using ComedyPull.Domain.Interfaces.Data;
using ComedyPull.Domain.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Domain.Core.Venues
{
    public class VenueService(IComedyDataSession session, ILogger<VenueService> logger)
    {
        public async Task<BatchProcessResult<ProcessedVenue, Venue>> ProcessVenuesAsync(IEnumerable<ProcessedVenue> processedVenues, CancellationToken ct)
        {
            var venues = processedVenues.ToList();
            var slugs = venues.Select(s => s.Slug).Distinct().Where(s => !string.IsNullOrEmpty(s));
            
            var existingVenues = await session.Venues.Query()
                .Where(v => slugs.Contains(v.Slug))
                .ToDictionaryAsync(v => v.Slug, v => v, ct);
            
            var toCreate = new List<Venue>();
            var toUpdate = new List<Venue>();

            foreach (var current in venues)
            {
                if (current.Slug is null)
                {
                    logger.LogWarning("Could not process venue because it's slug is null.");
                    continue;
                }
                
                if (existingVenues.TryGetValue(current.Slug, out var existing))
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
                await session.Venues.AddRangeAsync(toCreate, ct);
            
            if (toUpdate.Count != 0)
                session.Venues.UpdateRange(toUpdate);
            
            await session.SaveChangesAsync(ct);
            
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