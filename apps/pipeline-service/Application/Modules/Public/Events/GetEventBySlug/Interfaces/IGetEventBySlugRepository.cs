using ComedyPull.Domain.Modules.Common;

namespace ComedyPull.Application.Modules.Public.Events.GetEventBySlug.Interfaces
{
    public interface IGetEventBySlugRepository
    {
        public Task<Event?> GetEventBySlugAsync(string slug, CancellationToken cancellationToken);
    }
}