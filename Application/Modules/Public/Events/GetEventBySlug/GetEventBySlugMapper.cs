using ComedyPull.Application.Modules.Public.Shared;
using ComedyPull.Domain.Modules.Common;
using Riok.Mapperly.Abstractions;

namespace ComedyPull.Application.Modules.Public.Events.GetEventBySlug
{
    [Mapper]
    public partial class GetEventBySlugMapper
    {
        [MapperIgnoreSource(nameof(Event.Id))]
        [MapperIgnoreSource(nameof(Event.Source))]
        [MapperIgnoreSource(nameof(Event.VenueId))]
        [MapperIgnoreSource(nameof(Event.ComedianEvents))]
        [MapperIgnoreSource(nameof(Event.IngestedAt))]
        [MapperIgnoreSource(nameof(Event.CreatedAt))]
        [MapperIgnoreSource(nameof(Event.CreatedBy))]
        [MapperIgnoreSource(nameof(Event.UpdatedAt))]
        [MapperIgnoreSource(nameof(Event.UpdatedBy))]
        public partial GetEventBySlugResponse ToResponse(Event @event);
        
        private VenueResponse MapVenue(Venue venue) => new(
            venue.Slug,
            venue.Name
        );

        private List<ComedianResponse> MapComedians(ICollection<Comedian> comedians) =>
            comedians.Select(c => new ComedianResponse(
                c.Slug,
                c.Name,
                c.Bio
            )).ToList();
    }
}