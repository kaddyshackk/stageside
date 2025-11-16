using System.Text.Json;
using StageSide.Domain.Models;
using StageSide.Pipeline.Domain.Pipeline;
using StageSide.Pipeline.Domain.Pipeline.Models;
using StageSide.Pipeline.Domain.PipelineAdapter;
using StageSide.Pipeline.Domain.Sources.Punchup.Models;

namespace StageSide.Pipeline.Domain.Sources.Punchup
{
    public class PunchupTicketsPageTransformer : ITransformer
    {
        public ICollection<ProcessedEntity> Transform(object data)
        {
            var transformed = new List<ProcessedEntity>();

            if (data is not JsonElement jsonElement)
            {
                throw new ArgumentException("Expected JsonElement data");
            }
            
            var punchupRecord = JsonSerializer.Deserialize<PunchupRecord>(jsonElement.GetRawText());
            if (punchupRecord == null)
            {
                throw new Exception("Failed to deserialize punchup record from JSON.");
            }
            
            var now = DateTimeOffset.UtcNow;
            
            // Process Act
            var comedianSlug = SlugGenerator.GenerateSlug(punchupRecord.Name);
            var processedAct = new ProcessedAct
            {
                Name = punchupRecord.Name,
                Slug = comedianSlug,
                Bio = punchupRecord.Bio,
                ProcessedAt = now
            };
            
            transformed.Add(new ProcessedEntity
            {
                Type = EntityType.Act,
                Data = processedAct,
            });

            // Process Venues
            var uniqueVenues = punchupRecord.Events
                .Select(e => e.Venue)
                .Distinct()
                .ToList();

            var venueEntities = from venueName in uniqueVenues
                let venueSlug = SlugGenerator.GenerateSlug(venueName)
                select new ProcessedVenue { Name = venueName, Slug = venueSlug, ProcessedAt = now }
                into processedVenue
                select new ProcessedEntity
                {
                    Type = EntityType.Venue,
                    Data = processedVenue,
                };
            
            transformed.AddRange(venueEntities);


            // Process Events
            var eventEntities = from punchupEvent in punchupRecord.Events
                let venueSlug = SlugGenerator.GenerateSlug(punchupEvent.Venue)
                let eventSlug = SlugGenerator.GenerateEventSlug(comedianSlug, venueSlug, punchupEvent.StartDateTime)
                select new ProcessedEvent
                {
                    // TODO: Move to domain service
                    Title = $"{punchupRecord.Name} at {punchupEvent.Venue}",
                    Slug = eventSlug,
                    ComedianSlug = comedianSlug,
                    VenueSlug = venueSlug,
                    StartDateTime = punchupEvent.StartDateTime,
                    EndDateTime = null,
                    TicketLink = punchupEvent.TicketLink
                }
                into processedEvent
                select new ProcessedEntity
                {
                    Type = EntityType.Event,
                    Data = processedEvent
                };
            
            transformed.AddRange(eventEntities);

            return transformed;
        }
    }
}