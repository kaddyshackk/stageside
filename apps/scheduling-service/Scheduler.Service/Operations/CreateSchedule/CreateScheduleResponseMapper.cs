using Riok.Mapperly.Abstractions;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Service.Operations.CreateSchedule
{
    [Mapper]
    public partial class CreateScheduleResponseMapper
    {
        [MapperIgnoreSource("Source")]
        [MapperIgnoreSource("Sku")]
        [MapperIgnoreSource("Jobs")]
        [MapperIgnoreSource("CreatedAt")]
        [MapperIgnoreSource("CreatedBy")]
        [MapperIgnoreSource("UpdatedAt")]
        [MapperIgnoreSource("UpdatedBy")]
        public partial CreateScheduleResponse MapToResponse(Schedule schedule);
    }
}