using StageSide.Pipeline.Domain.Operations;
using Riok.Mapperly.Abstractions;

namespace StageSide.Pipeline.Service.Operations.Scheduling.CreateSchedule
{
    [Mapper]
    public partial class CreateScheduleRequestMapper
    {
        public partial CreateScheduleCommand MapToCommand(CreateScheduleRequest req);
    }
}