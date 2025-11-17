using Riok.Mapperly.Abstractions;
using StageSide.Scheduling.Models;

namespace StageSide.Pipeline.Service.Operations.Scheduling.CreateSchedule
{
    [Mapper]
    public partial class CreateScheduleResponseMapper
    {
        [MapperIgnoreSource("Sitemaps")]
        [MapperIgnoreSource("Jobs")]
        [MapperIgnoreSource("CreatedAt")]
        [MapperIgnoreSource("CreatedBy")]
        [MapperIgnoreSource("UpdatedAt")]
        [MapperIgnoreSource("UpdatedBy")]
        public partial CreateScheduleResponse MapToResponse(Schedule schedule);
    }
}