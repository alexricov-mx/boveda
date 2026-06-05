using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IUsuariosRepository
    {
        Task<DataResult<UsuariosFichaResponseDto>> GetUsuarioFichaAsync(string UserName, string Token);

        Task<DataResult<UsuarioSimpleDto>> GetFichaNombreAsync(string token);

        Task<DataResult<UsuarioSIODto>> GetUsuarioSIO(string ficha);

        Task<DataResult<IEnumerable<UsuariosNaResponseDto>>> GetUsuarioNAAsync(string UserName, string creditorNumber);

        Task<DataResult<IEnumerable<UsuariosNaResponseDto>>> GetUsuarioUiDAsync(string UserName, Guid userId);

        Task<DataResult<UsersDto>> GetUsuarioEmailAsync(string UserName, string Email);

        Task<DataResult<UsuariosPemexInDto>> ActualizaCambioAsync(UsuariosPemexInDto pemexDto);

        Task<DataResult<UsuariosPemexInDto>> ActualizaCambioNaAsync(UsuariosPemexInDto proveedorDto);

        Task<DataResult<UsersDto>> ActualizaBloqueaAsync(string UserNameModifier, Guid userId);

        Task<DataResult<UsersDto>> ActualizaAdministracionAsync(string UserName, Guid userId);

        Task<DataResult<UsuarioActualizaResponseDto>> ActualizaAdministracionNaAsync(string UserName, Guid userId);

        Task<DataResult<UsersDto>> ActualizaAdminAsync(string UserName, Guid userId);

        Task<DataResult<UsuariosPemexInDto>> InsertaUsuarioAsync(UsuariosPemexInDto pemexInDto);
        Task<DataResult<UsuariosPemexInDto>> InsertaUsuarioNaAsync(UsuariosPemexInDto proveedorInDto);
        Task<DataResult<UsuariosPemexInDto>> InsertaUsuarioAuditorPPIAsync(UsuariosPemexInDto auditorPpiDto);
        Task<DataResult<UsuariosPemexInDto>> ActualizaUsuarioAuditorPPIAsync(UsuariosPemexInDto auditorPpiDto);
        Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosByCreditorNumber(string CreditorNumber);
        Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosByCreditorBanking(string CreditorBanking);
        Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosByToken(string Token);

        Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosByCopadeId(string CopadeId);

        Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosAsync(string search = null);

        Task<DataResult<IEnumerable<UsersDto>>> GetUsuariosByUserId(Guid UserId);

        Task<UsersDto> GetUsuarioByESignId(int usuarioId);

        Task<DataResult<UsersDto>> GetUsuarioByIdEdit(Guid UserID);
        Task<UsersDto?> GetUserByEmailAsync(string Email);


    }
}

