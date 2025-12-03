using Riok.Mapperly.Abstractions;
using StageSide.SpaCollector.Domain.Models;

namespace StageSide.SpaCollector.Service.Operations.CreateSpaConfig
{
    [Mapper]
    public partial class CreateSpaConfigResponseMapper
    {
        [MapperIgnoreSource("CreatedAt")]
        [MapperIgnoreSource("CreatedBy")]
        [MapperIgnoreSource("UpdatedAt")]
        [MapperIgnoreSource("UpdatedBy")]
        public partial CreateSpaConfigResponse MapToResponse(SpaConfig config);

        [MapperIgnoreSource("SpaConfig")]
        [MapperIgnoreSource("IsActive")]
        [MapperIgnoreSource("CreatedAt")]
        [MapperIgnoreSource("CreatedBy")]
        [MapperIgnoreSource("UpdatedAt")]
        [MapperIgnoreSource("UpdatedBy")]
        public partial CreateSpaConfigSitemapResponse MapSitemapToResponse(Sitemap sitemap);
    }
}
