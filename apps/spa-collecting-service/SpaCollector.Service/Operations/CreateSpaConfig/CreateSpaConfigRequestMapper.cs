using Riok.Mapperly.Abstractions;
using StageSide.SpaCollector.Domain.Operations.CreateConfiguration;

namespace StageSide.SpaCollector.Service.Operations.CreateSpaConfig
{
    [Mapper]
    public partial class CreateSpaConfigRequestMapper
    {
        public partial CreateSpaConfigCommand MapToCommand(CreateSpaConfigRequest req);
    }
}