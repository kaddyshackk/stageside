using Riok.Mapperly.Abstractions;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Service.Operations.GetSource
{
    [Mapper]
    public partial class GetSourceResponseMapper
    {
        [MapperIgnoreSource("CreatedAt")]
        [MapperIgnoreSource("CreatedBy")]
        [MapperIgnoreSource("UpdatedAt")]
        [MapperIgnoreSource("UpdatedBy")]
        public partial GetSourceResponse MapToResponse(Source source);

        [MapperIgnoreSource("SourceId")]
        [MapperIgnoreSource("Schedules")]
        [MapperIgnoreSource("CreatedAt")]
        [MapperIgnoreSource("CreatedBy")]
        [MapperIgnoreSource("UpdatedAt")]
        [MapperIgnoreSource("UpdatedBy")]
        public partial GetSourceSkuResponse MapToSkuResponse(Sku sku);
    }
}