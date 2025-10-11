using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataProcessing.Services.Interfaces
{
    public interface ISubProcessorResolver
    {
        ISubProcessor<TKey> Resolve<TKey>(TKey key, ProcessingState fromState, ProcessingState toState) where TKey : struct;
    }
}