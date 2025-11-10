using ComedyPull.Domain.Operations;
using Riok.Mapperly.Abstractions;

namespace ComedyPull.Service.Operations.Scheduling.CreateJob
{
    [Mapper]
    public partial class CreateJobRequestMapper
    {
        public partial CreateJobCommand MapToCommand(CreateJobRequest req);
    }
}