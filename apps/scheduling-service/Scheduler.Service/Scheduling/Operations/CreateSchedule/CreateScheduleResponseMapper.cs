using Riok.Mapperly.Abstractions;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Service.Scheduling.Operations.CreateSchedule
{
    [Mapper]
    public partial class CreateScheduleResponseMapper
    {
        [MapperIgnoreSource("Sku")]
        [MapperIgnoreSource("Jobs")]
        [MapperIgnoreSource("CreatedAt")]
        [MapperIgnoreSource("CreatedBy")]
        [MapperIgnoreSource("UpdatedAt")]
        [MapperIgnoreSource("UpdatedBy")]
        public partial CreateScheduleResponse MapToResponse(Schedule schedule);
    }
}