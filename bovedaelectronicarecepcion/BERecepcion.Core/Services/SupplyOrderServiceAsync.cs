using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Exceptions;
using BERecepcion.Core.Interfaces.Auth;
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
    private readonly IUsuariosRepository _usuariosRepository;
    private readonly ICurrentUserService _currentUserService;
    public SupplyOrderServiceAsync(
        ISupplyOrderRepository supplyOrderRepositoryAsync,
        IUsuariosRepository usuariosRepository,
        ICurrentUserService currentUserService
        )
    {
        _supplyOrderRepositoryAsync = supplyOrderRepositoryAsync;
        _usuariosRepository = usuariosRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<SupplyOrderDto>>> GetPaginatedSupplyOrderAsync(
        SupplyOrderPagedRequest request,
        CancellationToken cancellationToken
        )
    {
        var user = await _usuariosRepository.GetUserByEmailAsync(_currentUserService.Email) 
            ?? throw new NotFoundException("Usuario no encontrado");

        var result = await _supplyOrderRepositoryAsync
            .GetListPaginatedSupplyOrderAsync(
            user.Token ?? string.Empty,
            request.PageSize,
            request.PageNumber,
            request.Search,
            cancellationToken);
        return Result.Success(result);
    }

    public async Task<Result<PagedResult<SupplyOrderDto>>> GetPaginatedSupplyOrderByProveedorAsync(
        ProvedorSupplyOrderPagedRequest request, 
        CancellationToken cancellationToken
        )
    {
        var user = await _usuariosRepository.GetUserByEmailAsync(_currentUserService.Email)
            ?? throw new NotFoundException("Usuario no encontrado");

        var result = await _supplyOrderRepositoryAsync
            .GetListPaginatedSupplyOrderByProveedorAsync(
            user.CreditorNumber ?? string.Empty,
            request.PageSize,
            request.PageNumber,
            request.Search,
            cancellationToken);
        return Result.Success(result);

    }
}
