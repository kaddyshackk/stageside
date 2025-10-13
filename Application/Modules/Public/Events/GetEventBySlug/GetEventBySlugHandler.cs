using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Modules.Public.Events.GetEventBySlug.Interfaces;

namespace ComedyPull.Application.Modules.Public.Events.GetEventBySlug
{
    public class GetEventBySlugHandler(IGetEventBySlugRepository repository) : IHandler<GetEventBySlugQuery, GetEventBySlugResponse>
    {
        public async Task<GetEventBySlugResponse?> HandleAsync(GetEventBySlugQuery request, CancellationToken cancellationToken)
        {
            var @event = await repository.GetEventBySlugAsync(request.Slug, cancellationToken);
            var mapper = new GetEventBySlugMapper();
            return @event is null ? null : mapper.ToResponse(@event);
        }
    }
}