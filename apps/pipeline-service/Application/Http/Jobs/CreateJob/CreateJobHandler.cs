using ComedyPull.Domain.Jobs.Operations.CreateJob;
using ComedyPull.Domain.Jobs.Services;

namespace ComedyPull.Application.Http.Jobs.CreateJob
{
    public class CreateJobHandler(JobAggregateService jobService) : IHandler<CreateJobCommand, CreateJobResponse>
    {
        public async Task<CreateJobResponse> HandleAsync(CreateJobCommand request, CancellationToken stoppingToken)
        {
            var job = await jobService.CreateJobAsync(request, stoppingToken);
            // TODO: Handle error state where job failed to create
            return new CreateJobResponseMapper().MapToResponse(job);
        }
    }
}