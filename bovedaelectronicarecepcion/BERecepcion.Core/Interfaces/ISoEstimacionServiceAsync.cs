using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Estimaciones.Dtos;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Interfaces;

public interface ISoEstimacionServiceAsync
{
    Task<Result<PagedResult<SOEstimationInternoDto>>> GetSOEInternoPaginationAsync(
        SOEstimationInternoRequestDto request,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<SOEstimationDto>>> GetSOEProveedorPaginationAsync(
        SOEstimationProveedorRequestDto request,
        CancellationToken cancellationToken = default);
}
