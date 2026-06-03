using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.ExpedienteElectronico.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Consulta.Interfaces.Repositories
{
    public interface IExpedienteElectronicoRepositoryAsync
    {
        Task<DataResult<ExpedienteEViewModel>> ExpedienteElectronico(string SAPOrder = null, Guid? CopadeID = null, Guid? AnaliticoPagoID = null);
        Task<ExpedienteEViewModel> GetExpedienteElectronico(ExpedienteElectronicoRequest request, CancellationToken cancellationToken);

    }
}
