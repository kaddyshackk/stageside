using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Modules.Public.Events.GetEventBySlug;

namespace ComedyPull.Api.Modules.Public
{
    public static class EventEndpoints
    {
        public static void AddEventEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/events").WithTags("Events");

            group.MapGet("/{slug}",
                    async (
                        string slug, IHandler<GetEventBySlugQuery, GetEventBySlugResponse> handler,
                        CancellationToken ct) =>
                    {
                        var @event = await handler.HandleAsync(new GetEventBySlugQuery(slug), ct);
                        return @event is null ? Results.NotFound() : Results.Ok(@event);
                    })
                .WithName("GetEventBySlug")
                .WithOpenApi();
        }
    }
}