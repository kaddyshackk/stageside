using Riok.Mapperly.Abstractions;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Service.Operations.CreateSource
{
    [Mapper]
    public partial class CreateSourceResponseMapper
    {
        [MapperIgnoreSource("Skus")]
        [MapperIgnoreSource("CreatedBy")]
        [MapperIgnoreSource("CreatedAt")]
        [MapperIgnoreSource("UpdatedBy")]
        [MapperIgnoreSource("UpdatedAt")]
        public partial CreateSourceResponse MapToResponse(Source source);
    }
}