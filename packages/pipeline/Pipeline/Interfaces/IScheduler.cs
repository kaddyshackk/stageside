using StageSide.Pipeline.Models;

namespace StageSide.Pipeline.Interfaces
{
    public interface IScheduler
    {
        public Task<ICollection<PipelineContext>> ScheduleAsync();
    }
}