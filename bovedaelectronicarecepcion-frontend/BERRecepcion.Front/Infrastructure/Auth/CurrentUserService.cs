using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace BERRecepcion.Front.Infrastructure.Auth;

/// <summary>
/// Implementación del servicio de acceso al usuario actual con caché por request.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private UsersDto? _cachedUser;
    private bool _userLoaded;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public UsersDto? GetCurrentUser()
    {
        // Caché lazy: solo deserializar una vez por request
        if (_userLoaded)
        {
            return _cachedUser;
        }

        _userLoaded = true;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null || !httpContext.User.Identity?.IsAuthenticated == true)
        {
            return null;
        }

        var userClaim = httpContext.User.FindFirst(ClaimConstants.UserClaim);
        if (userClaim == null || string.IsNullOrWhiteSpace(userClaim.Value))
        {
            return null;
        }

        try
        {
            _cachedUser = JsonConvert.DeserializeObject<UsersDto>(userClaim.Value);
            return _cachedUser;
        }
        catch (JsonException ex)
        {
            Serilog.Log.Error($"Error al deserializar claim User: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc />
    public bool HasRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null || !httpContext.User.Identity?.IsAuthenticated == true)
        {
            return false;
        }

        var rolesClaim = httpContext.User.FindFirst(ClaimConstants.RolesClaim);
        if (rolesClaim == null || string.IsNullOrWhiteSpace(rolesClaim.Value))
        {
            return false;
        }

        var userRoles = rolesClaim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return userRoles.Contains(role, StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public bool HasAnyRole(params string[] roles)
    {
        if (roles == null || roles.Length == 0)
        {
            return false;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null || !httpContext.User.Identity?.IsAuthenticated == true)
        {
            return false;
        }

        var rolesClaim = httpContext.User.FindFirst(ClaimConstants.RolesClaim);
        if (rolesClaim == null || string.IsNullOrWhiteSpace(rolesClaim.Value))
        {
            return false;
        }

        var userRoles = rolesClaim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return roles.Any(role => userRoles.Contains(role, StringComparer.Ordinal));
    }
}
