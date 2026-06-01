using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.ExpedienteElectronico.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Interfaces;

public interface IExpedienteElectronicoServiceAsync
{
    Task<Result<ExpedienteEViewModel>> GetExpedienteAsync(
        ExpedienteElectronicoRequest request,
        CancellationToken cancellationToken = default);
}
