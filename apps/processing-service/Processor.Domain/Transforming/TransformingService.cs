using Microsoft.Extensions.Logging;
using StageSide.Pipeline.Interfaces;
using StageSide.Pipeline.Models;

namespace StageSide.Processor.Domain.Transforming;

public class TransformingService(IPipelineAdapterFactory adapterFactory, ILogger<TransformingService> logger)
{
    private void TransformBatch(ICollection<PipelineContext> batch)
    {
        // Deserialize RawData back to object
        // object? deserializedData = null;
        // if (context.RawData is { } jsonString && !string.IsNullOrEmpty(jsonString))
        // {
        //     deserializedData = JsonSerializer.Deserialize<JsonElement>(jsonString);
        // }
        // else
        // {
        //     deserializedData = context.RawData;
        // }
        //
        // context.ProcessedEntities = transformer.Transform(deserializedData);
        // context.State = ProcessingState.Transformed;
    }
}