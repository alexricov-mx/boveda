using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Estimaciones.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Interfaces;

public interface ISoEstimacionServiceAsync
{
    Task<Result<PagedResult<SOEstimationInternoDto>>> GetSOEInternoPaginationAsync(
        SOEstimationInternoRequestDto request,
        CancellationToken cancellationToken = default);
}
