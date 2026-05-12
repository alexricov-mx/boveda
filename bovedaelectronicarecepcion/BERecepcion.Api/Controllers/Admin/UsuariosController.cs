using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Correos.Dto;

namespace BERecepcion.Api.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosRepository _usuariosRepository;
        private readonly IBitacoraAdmonRepository _bitacoraAdmonRepository;
        private readonly ICorreoRepository _correoRepository;

        public UsuariosController(IUsuariosRepository usuariosRepository, IBitacoraAdmonRepository bitacoraAdmonRepository, ICorreoRepository correoRepository)
        {
            _usuariosRepository = usuariosRepository;
            _bitacoraAdmonRepository = bitacoraAdmonRepository;
            _correoRepository = correoRepository;
        }

        /// <summary>
        /// Consulta de usuarios con o sin filtro
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet("{search}")]
        [ProducesResponseType(typeof(DataResult<UsersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsuariosAsync(string search = null)
        {
            var result = await _usuariosRepository.GetUsuariosAsync(search);
            return result.ToActionResult();
        }

        /// <summary>
        /// Bloquear o desbloquear usuarios
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="UserName"></param>
        /// <param name="Name"></param>
        /// <param name="UserNameModifier"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("Estatus/{UserNameModifier}/{UserName}/{Name}/{Email}/{userId}")]
        [ProducesResponseType(typeof(DataResult<UsersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaBloqueaAsync(string Email, string UserName, string Name, string UserNameModifier, Guid userId)
        {
            var result = await _usuariosRepository.ActualizaBloqueaAsync(UserNameModifier, userId);
            
            if (!result.Data.IsBlocked)
            {
                var correo = new CorreoDto
                {
                    To = Email,
                    Subject = "Desbloqueo de usuario",
                    Accion = "Desbloqueo de usuario",
                    Nombre = Name,
                    userName = UserName
                };

                try
                {
                    await _correoRepository.EnvioCorreoUserAsync(correo);
                }
                catch (Exception ex)
                {
                    string errMsg = ex.Message;
                }
            }

            var usr = _usuariosRepository.GetUsuariosByUserId(userId).Result.Data;
            if (usr != null)
            {
                string status = result.Data.IsBlocked ? "Bloqueado" : "Desbloqueado";
                var bitacora = new BitacoraAdmonDto
                {
                    Evento = "Actualizacion status",
                    Usuario = UserName,
                    Descripcion = "Actualizacion status " + status + " - " + usr.FirstOrDefault().Name
                };
                await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
            }
            return result.ToActionResult();
        }

        /// <summary>
        /// Cambiar estatus de usuario entre Administrador / Funcionario / Proveedor
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("Administra/{UserName}/{userId}")]
        [ProducesResponseType(typeof(DataResult<UsersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaAdminAsync(string UserName, Guid userId)
        {
            try
            {
                var result = await _usuariosRepository.ActualizaAdminAsync(UserName, userId);
                if (result.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("ActualizaAdminAsync: {error}", result.Message);
                    return Ok(result);
                }
                var bitacora = new BitacoraAdmonDto
                {
                    Evento = "Actualizacion userType",
                    Usuario = UserName,
                    Descripcion = "Actualizacion userType " + result.Data.UserType + " - " + userId.ToString() + " -- Usuario -" + UserName
                };
                await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("ActualizaAdminAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        /// <summary>
        /// Cambio de estatus de usuario a Super Usuario 
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("Administra/Pemex/{UserName}/{userId}")]
        [ProducesResponseType(typeof(DataResult<UsersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaAdministracionAsync(string UserName, Guid userId)
        {
            try
            {
                var result = await _usuariosRepository.ActualizaAdministracionAsync(UserName, userId);
                if (result.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("ActualizaAdministracionAsync: {error}", result.Message);
                    return Ok(result);
                }
                var usr = await _usuariosRepository.GetUsuariosByUserId(userId);
                if (usr.Data.FirstOrDefault() != null)
                {
                    var bitacora = new BitacoraAdmonDto
                    {
                        Evento = "Actualizacion userType",
                        Usuario = UserName,
                        Descripcion = "Actualizacion userType " + result.Data.UserType + " - " + usr.Data.FirstOrDefault().Name
                    };
                    await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("ActualizaAdministracionAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        /// <summary>
        /// Consulta de usuario por token
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("GetUsuarioByIdEdit/{UserID}")]
        [ProducesResponseType(typeof(DataResult<UsersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsuarioByIdEdit(Guid UserID)
        {
            try
            {
                var result = await _usuariosRepository.GetUsuarioByIdEdit(UserID);
                if (result.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("GetUsuarioByIdEdit: {error}", result.Message);
                    return Ok(result);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("GetUsuarioByIdEdit: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("Ficha/{UserName}/{Token}")]
        [ProducesResponseType(typeof(DataResult<UsuariosFichaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsuarioFichaAsync(string UserName, string Token)
        {
            try
            {
                return Ok(await _usuariosRepository.GetUsuarioFichaAsync(UserName, Token));
            }
            catch (Exception ex)
            {
                Log.Error("GetUsuarioFicha: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("Simple/{Token}")]
        [ProducesResponseType(typeof(DataResult<UsuariosFichaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFichaNombreAsync(string Token)
        {
            try
            {
                return Ok(await _usuariosRepository.GetFichaNombreAsync(Token));
            }
            catch (Exception ex)
            {
                Log.Error("GetUsuarioFicha: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("SIO/{ficha}")]
        [ProducesResponseType(typeof(DataResult<UsuarioSIODto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserSIOAsync(string ficha)
        {
            try
            {
                return Ok(await _usuariosRepository.GetUsuarioSIO(ficha));
            }
            catch (Exception ex)
            {
                Log.Error("GetUserSIO: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("Acreedores/{UserName}/{creditorNumber}")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<UsuariosNaResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsuarioNAAsync(string UserName, string creditorNumber)
        {
            try
            {
                return Ok(await _usuariosRepository.GetUsuarioNAAsync(UserName, creditorNumber));
            }
            catch (Exception ex)
            {
                Log.Error("GetUsuarioNA: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("Acreedor/{UserName}/{userId}")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<UsuariosNaResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsuarioUiDAsync(string UserName, Guid userId)
        {
            try
            {
                return Ok(await _usuariosRepository.GetUsuarioUiDAsync(UserName, userId));
            }
            catch (Exception ex)
            {
                Log.Error("GetUsuarioNA: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("Correo/{Email}")]
        [ProducesResponseType(typeof(DataResult<UsersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsuarioEmailAsync(string Email)
        {
            try
            {
                return Ok(await _usuariosRepository.GetUsuarioEmailAsync("", Email));
            }
            catch (Exception ex)
            {
                Log.Error("GetUsuarioEmail: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("Correo/{UserName}/{Email}")]
        [ProducesResponseType(typeof(DataResult<UsersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsuarioEmailAsync(string UserName, string Email)
        {
            try
            {
                return Ok(await _usuariosRepository.GetUsuarioEmailAsync(UserName, Email));
            }
            catch (Exception ex)
            {
                Log.Error("GetUsuarioEmail: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("Administra/proveedor/{UserName}/{userId}")]
        [ProducesResponseType(typeof(DataResult<UsuarioActualizaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaAdministracionNaAsync(string UserName, Guid userId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return Ok(await _usuariosRepository.ActualizaAdministracionNaAsync(UserName, userId));
                }
                catch (Exception ex)
                {
                    Log.Error("ActualizaAdministracion: {error}", ex.ToString());
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

        [HttpPost("Inserta/Pemex")]
        [ProducesResponseType(typeof(DataResult<UsuariosPemexInDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaUsuarioAsync(UsuariosPemexInDto pemexInDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _usuariosRepository.InsertaUsuarioAsync(pemexInDto);
                    if (result.Status == System.Net.HttpStatusCode.OK)
                    {
                        var correo = new CorreoDto
                        {
                            To = pemexInDto.Email,
                            Subject = "Creacion de Usuario",
                            Accion = "Creacion de Usuario",
                            Nombre = pemexInDto.Name,
                            userName = pemexInDto.userName
                        };

                        try
                        {
                            await _correoRepository.EnvioCorreoUserAsync(correo);
                        }
                        catch (Exception ex)
                        {
                            string errMsg = ex.Message;
                        }

                        var bitacora = new BitacoraAdmonDto
                        {
                            Evento = "Nuevo",
                            Usuario = pemexInDto.nombreLogeado,
                            Descripcion = "Se dio de alta al Funcionario - " + pemexInDto.Ficha + " -- Usuario -" + pemexInDto.Ficha
                        };

                        await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);

                    }
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    Log.Error("InsertaUsuarioPemex: {error}", ex.ToString());
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

        [HttpPost("Actualiza/Pemex")]
        [ProducesResponseType(typeof(DataResult<UsuariosPemexInDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaCambioAsync([FromBody] UsuariosPemexInDto pemexDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _usuariosRepository.ActualizaCambioAsync(pemexDto);
                    if (result.Status == System.Net.HttpStatusCode.OK)
                    {
                        var bitacora = new BitacoraAdmonDto
                        {
                            Evento = "Editar",
                            Usuario = pemexDto.nombreLogeado,
                            Descripcion = "Se modifico al Funcionario - " + pemexDto.Ficha + " -- Usuario -" + pemexDto.Ficha
                        };
                        await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                    }
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    Log.Error("ActualizaCambio: {error}", ex.ToString());
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

        [HttpPost("Inserta/Proveedor")]
        [ProducesResponseType(typeof(DataResult<UsuariosPemexInDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaUsuarioNaAsync(UsuariosPemexInDto proveedorInDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _usuariosRepository.InsertaUsuarioNaAsync(proveedorInDto);
                    if (result.Status == System.Net.HttpStatusCode.OK)
                    {
                        var bitacora = new BitacoraAdmonDto
                        {
                            Evento = "Nuevo",
                            Usuario = proveedorInDto.nombreLogeado,
                            Descripcion = "Se dio de alta al Proveedor - " + proveedorInDto.Name
                        };
                        await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                    }
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    Log.Error("InsertaUsuarioProveedor: {error}", ex.ToString());
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

        [HttpPost("Actualiza/Proveedor")]
        [ProducesResponseType(typeof(DataResult<UsuariosPemexInDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaCambioNaAsync([FromBody] UsuariosPemexInDto proveedorDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _usuariosRepository.ActualizaCambioNaAsync(proveedorDto);
                    if (result.Status == System.Net.HttpStatusCode.OK)
                    {
                        var bitacora = new BitacoraAdmonDto
                        {
                            Evento = "Editar",
                            Usuario = proveedorDto.nombreLogeado,
                            Descripcion = "Se modifico al Proveedor - " + proveedorDto.Name
                        };
                        await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                    }
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    Log.Error("ActualizaCambio: {error}", ex.ToString());
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

        [HttpPost("Inserta/AuditorPPI")]
        [ProducesResponseType(typeof(DataResult<UsuariosPemexInDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaUsuarioAuditorPPIAsync(UsuariosPemexInDto auditorPpiDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _usuariosRepository.InsertaUsuarioAuditorPPIAsync(auditorPpiDto);
                    if (result.Status == System.Net.HttpStatusCode.OK)
                    {
                        if (auditorPpiDto.userType == "Auditor")
                        {
                            var bitacora = new BitacoraAdmonDto
                            {
                                Evento = "Nuevo",
                                Usuario = auditorPpiDto.nombreLogeado,
                                Descripcion = "Se dio de alta al Auditor - " + auditorPpiDto.Name + " -- Usuario -" + auditorPpiDto.Name
                            };
                            await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                        }
                        else
                        {
                            var bitacora = new BitacoraAdmonDto
                            {
                                Evento = "Nuevo",
                                Usuario = auditorPpiDto.nombreLogeado,
                                Descripcion = "Se dio de alta al UsuarioPPI - " + auditorPpiDto.Name + " -- Usuario -" + auditorPpiDto.Name
                            };
                            await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                        }
                    }
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    Log.Error("InsertaUsuarioAuditorPpi: {error}", ex.ToString());
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

        [HttpPost("Actualiza/AuditorPPIAsync")]
        [ProducesResponseType(typeof(DataResult<UsuariosPemexInDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaUsuarioAuditorPPIAsync([FromBody] UsuariosPemexInDto auditorPpiDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _usuariosRepository.ActualizaUsuarioAuditorPPIAsync(auditorPpiDto);
                    if (result.Status == System.Net.HttpStatusCode.OK)
                    {
                        if (auditorPpiDto.userType == "Auditor")
                        {
                            var bitacora = new BitacoraAdmonDto
                            {
                                Evento = "Editar",
                                Usuario = auditorPpiDto.nombreLogeado,
                                Descripcion = "Se dio de alta al Auditor - " + auditorPpiDto.Name + " -- Usuario -" + auditorPpiDto.Name
                            };
                            await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                        }
                        else
                        {
                            var bitacora = new BitacoraAdmonDto
                            {
                                Evento = "Editar",
                                Usuario = auditorPpiDto.nombreLogeado,
                                Descripcion = "Se dio de alta al UsuarioPPI - " + auditorPpiDto.Name + " -- Usuario -" + auditorPpiDto.Name
                            };
                            await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                        }
                    }
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    Log.Error("InsertaUsuarioAuditorPpi: {error}", ex.ToString());
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }


    }
}
