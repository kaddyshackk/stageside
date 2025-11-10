using StageSide.Pipeline.Domain.Operations;
using Riok.Mapperly.Abstractions;

namespace StageSide.Pipeline.Service.Operations.Scheduling.CreateJob
{
    [Mapper]
    public partial class CreateJobRequestMapper
    {
        public partial CreateJobCommand MapToCommand(CreateJobRequest req);
    }
}