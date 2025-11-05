using ComedyPull.Domain.Jobs.Operations.CreateJob;
using Riok.Mapperly.Abstractions;

namespace ComedyPull.Api.Http.Jobs.CreateJob
{
    [Mapper]
    public partial class CreateJobRequestMapper
    {
        public partial CreateJobCommand MapToCommand(CreateJobRequest req);
    }
}