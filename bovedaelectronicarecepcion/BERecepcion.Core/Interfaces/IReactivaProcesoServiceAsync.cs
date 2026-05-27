using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Dto;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Interfaces;

public interface IReactivaProcesoServiceAsync
{
    Task<Result<IEnumerable<ReactivaProcesosResponseDto>>> GetReactivaProcessAsync(string SAPOrder, CancellationToken cancellationToken);
    Task<Result<ReactivaProcesosDto>> GetSAPFirmaAsync(string SAPOrder, CancellationToken cancellationToken);

}
