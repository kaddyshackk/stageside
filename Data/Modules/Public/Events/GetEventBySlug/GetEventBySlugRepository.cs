using ComedyPull.Application.Modules.Public.Events.GetEventBySlug.Interfaces;
using ComedyPull.Domain.Modules.Common;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.Public.Events.GetEventBySlug
{
    public class GetEventBySlugRepository(GetEventBySlugContext context) : IGetEventBySlugRepository
    {
        public async Task<Event?> GetEventBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return await context.Events.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        }
    }
}