using BERecepcion.Core.Common.Results;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Interfaces.Repositories;

public interface ISupplyOrderServiceAsync
{
    Task<Result<PagedResult<SupplyOrderDto>>> GetPaginatedSupplyOrderAsync(
        SupplyOrderPagedRequest request,
        CancellationToken cancellationToken
        );
    Task<Result<PagedResult<SupplyOrderDto>>> GetPaginatedSupplyOrderByProveedorAsync(
        SupplyOrderPagedRequest request,
        CancellationToken cancellationToken
        );
}
