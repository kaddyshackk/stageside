using StageSide.Pipeline.Domain.Scheduling.Models;
using Riok.Mapperly.Abstractions;

namespace StageSide.Pipeline.Service.Operations.Scheduling.CreateSchedule
{
    [Mapper]
    public partial class CreateScheduleResponseMapper
    {
        [MapperIgnoreSource("Sitemaps")]
        [MapperIgnoreSource("Executions")]
        [MapperIgnoreSource("CreatedAt")]
        [MapperIgnoreSource("CreatedBy")]
        [MapperIgnoreSource("UpdatedAt")]
        [MapperIgnoreSource("UpdatedBy")]
        public partial CreateScheduleResponse MapToResponse(Schedule schedule);
    }
}