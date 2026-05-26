using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IReactivaProcesosRepository
    {
        Task<DataResult<ReactivaProcesosDto>> GetEnvioFirmaAsync(string SAPOrder);
        Task<DataResult<JsonRP>> PIEnvioReactivaProcesosAsync(DataResult<JsonRP> datosRect);
        Task<DataResult<IEnumerable<ReactivaProcesosResponseDto>>> GetReactivaProcesosAsync(string OrderSAP);
        Task<IEnumerable<ReactivaProcesosResponseDto>> GetReactivaProcesosBySAPOrderAsync(string OrderSAP, CancellationToken cancellationToken);

    }
}
