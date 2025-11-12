using StageSide.Pipeline.Domain.Pipeline.Interfaces;

namespace StageSide.Pipeline.Domain.PipelineAdapter
{
    public interface IWebPageFactory
    {
        public IWebPage GetWebPage();
    }
}