using System.Text.Json;
using ComedyPull.Domain.Core.Events.Services;
using ComedyPull.Domain.Core.Shared;
using ComedyPull.Domain.Core.Shared.Services;
using ComedyPull.Domain.Pipeline;
using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Sources.Punchup.Models;

namespace ComedyPull.Domain.Sources.Punchup
{
    public class PunchupTicketsPageTransformer : ITransformer
    {
        public ICollection<ProcessedEntity> Transform(object data)
        {
            var transformed = new List<ProcessedEntity>();

            var punchupRecord = (PunchupRecord)data;
            if (punchupRecord == null)
            {
                throw new Exception("Failed to process punchup record because it was null.");
            }
            
            var now = DateTimeOffset.UtcNow;
            
            // Process Act
            var comedianSlug = GenericSlugGenerator.GenerateSlug(punchupRecord.Name);
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
                let venueSlug = GenericSlugGenerator.GenerateSlug(venueName)
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
                let venueSlug = GenericSlugGenerator.GenerateSlug(punchupEvent.Venue)
                let eventSlug = EventSlugGenerator.GenerateEventSlug(comedianSlug, venueSlug, punchupEvent.StartDateTime)
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
                    Data = JsonSerializer.Serialize(processedEvent)
                };
            
            transformed.AddRange(eventEntities);

            return transformed;
        }
    }
}