using Riok.Mapperly.Abstractions;
using StageSide.Scheduler.Domain.Scheduling.Operations.CreateSchedule;

namespace StageSide.Scheduler.Service.Scheduling.Operations.CreateSchedule
{
    [Mapper]
    public partial class CreateScheduleRequestMapper
    {
        public partial CreateScheduleCommand MapToCommand(CreateScheduleRequest req);
    }
}