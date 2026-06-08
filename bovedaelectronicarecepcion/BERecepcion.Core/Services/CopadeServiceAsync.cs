using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Copades.Dto;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Exceptions;
using BERecepcion.Core.Interfaces;
using BERecepcion.Core.Interfaces.Auth;
using BERecepcion.Core.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Services;

public class CopadeServiceAsync(
    ICopadeRepository copadeRepository,
    IUsuariosRepository usuariosRepository,
    ICurrentUserService currentUserService)
    : ICopadeServiceAsync
{
    private readonly ICopadeRepository _copadeRepository = copadeRepository;
    private readonly IUsuariosRepository _usuariosRepository = usuariosRepository;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<PagedResult<CopadeDto>>> GetPagedFiltroCopadesAsync(
        CopadeFiltroRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _usuariosRepository.GetUserByEmailAsync(_currentUserService.Email)
            ?? throw new NotFoundException("Usuario no encontrado");

        var result = await _copadeRepository.GetPagedFiltroCopadesAsync(
            user.UserID.ToString(),
            request.PageSize,
            request.PageNumber,
            request.Search,
            cancellationToken);

        return Result.Success(
            new PagedResult<CopadeDto>(
                result.Items,
                result.TotalItems,
                result.PageNumber,
                result.PageSize));
    }
}
