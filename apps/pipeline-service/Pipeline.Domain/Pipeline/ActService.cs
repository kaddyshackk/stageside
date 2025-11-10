using StageSide.Pipeline.Domain.Models;
using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.Pipeline.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace StageSide.Pipeline.Domain.Pipeline
{
    public class ActService(IComedyDataSession session, ILogger<ActService> logger)
    {
        public async Task<BatchProcessResult<ProcessedAct, Act>> ProcessActsAsync(
            IEnumerable<ProcessedAct> processedActs, CancellationToken ct)
        {
            var acts = processedActs.ToList();
            var slugs = acts.Select(s => s.Slug).Distinct().Where(s => !string.IsNullOrEmpty(s));

            var existingActs = await session.Acts.Query()
                .Where(a => slugs.Contains(a.Slug))
                .ToDictionaryAsync(a => a.Slug, a => a, ct);

            var toCreate = new List<Act>();
            var toUpdate = new List<Act>();
            
            foreach (var current in acts)
            {
                if (current.Slug is null)
                {
                    logger.LogWarning("Could not process act because it's slug is null.");
                    continue;
                }
                if (existingActs.TryGetValue(current.Slug, out var existing))
                {
                    var hasChanges = false;
                    if (!string.IsNullOrEmpty(existing.Name) && existing.Name != current.Name)
                    {
                        existing.Name = current.Name;
                        hasChanges = true;
                    }
                    
                    if (!string.IsNullOrEmpty(current.Bio) && existing.Bio != current.Bio)
                    {
                        existing.Bio = current.Bio;
                        hasChanges = true;
                    }

                    if (!hasChanges) continue;
                    existing.UpdatedAt = DateTimeOffset.UtcNow;
                    existing.UpdatedBy = "System";
                    toUpdate.Add(existing);
                }
                else
                {
                    toCreate.Add(new Act
                    {
                        Slug = current.Slug,
                        Name = current.Name,
                        Bio = current.Bio,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = "System",
                        UpdatedAt = DateTimeOffset.UtcNow,
                        UpdatedBy = "System"
                    });
                }
            }
            
            if (toCreate.Count != 0)
                await session.Acts.AddRangeAsync(toCreate, ct);
            
            if (toUpdate.Count != 0)
                session.Acts.UpdateRange(toUpdate);
            
            await session.SaveChangesAsync(ct);
            
            return new BatchProcessResult<ProcessedAct, Act>
            {
                Created = toCreate,
                Updated = toUpdate,
                Failed = [],
                ProcessedCount = acts.Count
            };
        }
    }
}