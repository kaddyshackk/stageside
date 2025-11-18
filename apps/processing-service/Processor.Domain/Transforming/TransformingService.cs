using System.Text.Json;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using StageSide.Pipeline.Interfaces;
using StageSide.Pipeline.Models;

namespace StageSide.Processor.Domain.Transforming;

public class TransformingService(IPipelineAdapterFactory adapterFactory, ILogger<TransformingService> logger)
{
    private void TransformBatch(ICollection<PipelineContext> batch)
    {
        foreach (var context in batch)
        {
            using (LogContext.PushProperty("ContextId", context.Id))
            using (LogContext.PushProperty("ExecutionId", context.JobId))
            using (LogContext.PushProperty("Sku", context.SkuKey))
            using (LogContext.PushProperty("CollectionUrl", context.Metadata.CollectionUrl))
            using (LogContext.PushProperty("Tags", context.Metadata.Tags))
            {
                var adapter = adapterFactory.GetAdapter(context.SkuKey);
                var transformer = adapter.GetTransformer();

                try
                {
                    // Deserialize RawData back to object
                    object? deserializedData = null;
                    if (context.RawData is { } jsonString && !string.IsNullOrEmpty(jsonString))
                    {
                        deserializedData = JsonSerializer.Deserialize<JsonElement>(jsonString);
                    }
                    else
                    {
                        deserializedData = context.RawData;
                    }

                    context.ProcessedEntities = transformer.Transform(deserializedData);
                    context.State = ProcessingState.Transformed;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to transform context {ContextId} with sku {Sku}",
                        context.Id, context.SkuKey);
                    context.State = ProcessingState.Failed;
                }
            }
        }
    }
}