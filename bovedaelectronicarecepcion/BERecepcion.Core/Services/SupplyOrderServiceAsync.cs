using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Interfaces.Repositories;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Services;

public class SupplyOrderServiceAsync
    : ISupplyOrderServiceAsync
{
    private readonly ISupplyOrderRepository _supplyOrderRepositoryAsync;

    public SupplyOrderServiceAsync(
        ISupplyOrderRepository supplyOrderRepositoryAsync
        )
    {
        _supplyOrderRepositoryAsync = supplyOrderRepositoryAsync;
    }

    public async Task<Result<PagedResult<SupplyOrderDto>>> GetPaginatedSupplyOrderAsync(
        SupplyOrderPagedRequest request,
        CancellationToken cancellationToken
        )
    {
        var result = await _supplyOrderRepositoryAsync
            .GetListPaginatedSupplyOrderAsync(
            request, 
            cancellationToken);
        return Result.Success(result);
    }

    public async Task<Result<PagedResult<SupplyOrderDto>>> GetPaginatedSupplyOrderByProveedorAsync(
        ProvedorSupplyOrderPagedRequest request, 
        CancellationToken cancellationToken
        )
    {
        var result = await _supplyOrderRepositoryAsync
            .GetListPaginatedSupplyOrderByProveedorAsync(
            request,
            cancellationToken);
        return Result.Success(result);

    }
}
