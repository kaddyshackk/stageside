using ComedyPull.Application.Modules.Public.Events.GetEventBySlug.Interfaces;
using ComedyPull.Data.Modules.Common;
using ComedyPull.Domain.Modules.Common;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.Public
{
    public class GetEventBySlugRepository(ComedyPullContext context) : IGetEventBySlugRepository
    {
        public async Task<Event?> GetEventBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return await context.Events.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        }
    }
}