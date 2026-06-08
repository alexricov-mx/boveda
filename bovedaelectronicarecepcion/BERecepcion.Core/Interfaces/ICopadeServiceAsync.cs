using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Copades.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Interfaces;

public interface ICopadeServiceAsync
{
    Task<Result<PagedResult<CopadeDto>>> GetPagedFiltroCopadesAsync(
        CopadeFiltroRequest request,
        CancellationToken cancellationToken = default);
}
