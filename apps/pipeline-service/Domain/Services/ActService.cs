using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Domain.Services
{
    public class ActService(IServiceScopeFactory scopeFactory)
    {
        public async Task<BatchProcessResult<ProcessedAct, Act>> ProcessActsAsync(IEnumerable<ProcessedAct> processedActs)
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IActRepository>();
            
            var acts = processedActs.ToList();
            var slugs = acts.Select(s => s.Slug).Distinct().Where(s => !string.IsNullOrEmpty(s));
            var existingActs = await repository.GetActsBySlugAsync(slugs!);
            var existingBySlug = existingActs.ToDictionary(s => s.Slug, s => s);

            var toCreate = new List<Act>();
            var toUpdate = new List<Act>();

            foreach (var current in acts)
            {
                if (existingBySlug.TryGetValue(current.Slug!, out var existing))
                {
                    var hasChanges = false;
                    if (!string.IsNullOrEmpty(current.Name) && existing.Name != current.Name)
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
                await repository.BulkCreateActsAsync(toCreate);
            
            if (toUpdate.Count != 0)
                await repository.SaveChangesAsync();

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