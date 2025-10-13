using ComedyPull.Application.Modules.DataProcessing.Services.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataProcessing.Services
{
    public class SubProcessorResolver(IServiceProvider serviceProvider) : ISubProcessorResolver
    {
        public ISubProcessor<TKey> Resolve<TKey>(TKey key, ProcessingState fromState, ProcessingState toState) where TKey : struct
        {
            var subProcessors = serviceProvider.GetService(typeof(IEnumerable<ISubProcessor<TKey>>))
                as IEnumerable<ISubProcessor<TKey>> ?? Enumerable.Empty<ISubProcessor<TKey>>();

            // Try to find specific match
            var specificProcessor = subProcessors.FirstOrDefault(p =>
                Equals(p.Key, key) &&
                p.FromState == fromState &&
                p.ToState == toState);

            if (specificProcessor != null)
                return specificProcessor;

            // Fallback to generic (Key is null)
            var genericProcessor = subProcessors.FirstOrDefault(p =>
                p.Key == null &&
                p.FromState == fromState &&
                p.ToState == toState);

            if (genericProcessor != null)
                return genericProcessor;

            throw new InvalidOperationException(
                $"No sub-processor found for key '{key}', transition {fromState} -> {toState}");
        }
    }
}