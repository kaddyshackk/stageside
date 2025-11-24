using Riok.Mapperly.Abstractions;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Service.Operations.CreateSku
{
    [Mapper]
    public partial class CreateSkuResponseMapper
    {
        [MapperIgnoreSource("Schedules")]
        [MapperIgnoreSource("CreatedAt")]
        [MapperIgnoreSource("CreatedBy")]
        [MapperIgnoreSource("UpdatedAt")]
        [MapperIgnoreSource("UpdatedBy")]
        public partial CreateSkuResponse MapToResponse(Sku sku);
    }
}