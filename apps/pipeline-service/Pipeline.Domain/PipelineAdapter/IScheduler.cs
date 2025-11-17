using StageSide.Pipeline.Domain.Pipeline.Models;
using StageSide.Scheduling.Models;

namespace StageSide.Pipeline.Domain.PipelineAdapter
{
    public interface IScheduler
    {
        public Task<ICollection<PipelineContext>> ScheduleAsync(Schedule schedule, Job job, CancellationToken ct);
    }
}