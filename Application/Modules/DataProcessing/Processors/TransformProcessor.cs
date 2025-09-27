using ComedyPull.Application.Modules.DataProcessing.Processors.Interfaces;
using ComedyPull.Domain.Models.Processing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Modules.DataProcessing.Processors
{
    public class TransformProcessor(IMediator mediator, ILogger<TransformProcessor> logger)
        : BaseStateProcessor(mediator, logger), ITransformProcessor
    {
        public override ProcessingState FromState => ProcessingState.Ingested;
        public override ProcessingState ToState => ProcessingState.Transformed;

        protected override Task ProcessRecordsAsync(Guid batchId, CancellationToken cancellationToken)
        {
            logger.LogInformation($"Processing batch {batchId} to state {ToState}");
            return Task.CompletedTask;
        }
    }
}