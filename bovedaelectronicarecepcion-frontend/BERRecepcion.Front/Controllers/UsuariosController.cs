using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;
using Serilog;
using System.Linq;
using Azure;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace BERRecepcion.Front.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ILogger<UsuariosController> _logger;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public UsuariosController(ILogger<UsuariosController> logger, IRestUtility utility, IGenerals generals)
        {
            _logger = logger;
            _utility = utility;
            _generals = generals;
        }

        [RoleFilter(Roles: "AdministrationUsers")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Tabla con información de los usuarios que cumplen con el criterio de búsqueda.
        /// </summary>
        /// <param name="search">Criterio de búsqueda</param>
        /// <returns>En caso de exito de retorna vista parcial de la tabla o en su defecto mensaje de error.</returns>
        [HttpGet]
        public async Task<IActionResult> UserTable(string search = null)
        {
            try
            {
                var response = await _utility.GetItem<DataResult<IEnumerable<UsersDto>>>("Usuarios/" + search);
                _logger.LogInformation($"UserTable - response is null: {response == null}");
                if (response != null)
                {
                    _logger.LogInformation($"UserTable - response.Status: {response.Status}");
                    _logger.LogInformation($"UserTable - response.Data is null: {response.Data == null}");
                    if (response.Data != null)
                    {
                        _logger.LogInformation($"UserTable - response.Data.Count: {response.Data.Count()}");
                    }
                }
                
                if (response == null || response.Status != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogWarning($"UserTable - Retornando JSON de error porque response={response == null} o Status={response?.Status}");
                    return Json(new { success = false, message = "Ocurrió un error al procesar la información, por favor intente de nuevo." });
                }
                    
                _logger.LogInformation("UserTable - Retornando PartialView _UserTable");
                return PartialView("_UserTable", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserTable - Exception capturada");
                return Json(new { success = false, message = "Ocurrió un error al procesar la información, por favor intente de nuevo." });
            }
        }

        /// <summary>
        /// Actualiza estatus de usuario Activo / Bloqueado
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="name">Nombre de usuario</param>
        /// <param name="email">Correo electrónico de usuario</param>
        /// <param name="userId">Identificador de usuario</param>
        /// <returns>Mensaje de resultado de proceso</returns>
        [HttpPost]
        public async Task<JsonResult> StatusUsuarioPemex(string userName, string name, string email, Guid userId)
        {
            string UserNameModifier = _generals.User.UserType;
            try
            {
                var response = await _utility.GetItem<DataResult<UsersDto>>("Usuarios/Estatus/" + UserNameModifier + "/" + userName + "/" + name + "/" + email + "/" + userId);
                if (response == null || response.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al procesar la información, por favor intente de nuevo." });
                return Json(new { success = true, message = "Información actualizada correctamente." });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, message = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }

        /// <summary>
        /// Actualiza tipo de usuario Administrador: UserTypeA
        /// </summary>
        /// <param name="userId">Identificador de usuario</param>
        /// <returns>Mensaje de resultado de proceso</returns>
        [HttpPost]
        public async Task<IActionResult> AdminUsuario(Guid userId)
        {
            string UserName = _generals.User.UserType;

            try
            {
                var response = await _utility.GetItem<DataResult<UsersDto>>("Usuarios/Administra/" + UserName + "/" + userId);
                if (response == null || response.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al procesar la información, por favor intente de nuevo." });
                return Json(new { success = true, message = "Se actualizó el tipo de usuario correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, message = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }

        /// <summary>
        /// Actualiza tipo de usuairo Super Administrador: UserTypeS
        /// </summary>
        /// <param name="userId">Identificación de usuario</param>
        /// <returns>Mensaje de resultado de proceso</returns>
        [HttpPost]
        public async Task<JsonResult> SuperUsuario(Guid userId)
        {
            string UserName = _generals.User.UserType;
            try
            {
                var response = await _utility.GetItem<DataResult<UsersDto>>("Usuarios/Administra/Pemex/" + UserName + "/" + userId);
                if (response == null || response.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al procesar la información, por favor intente de nuevo." });
                return Json(new { success = true, message = "Se actualizó el tipo de usuario correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, message = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }

        /// <summary>
        /// Muestra el modal para creación edición de usuario
        /// </summary>
        /// <param name="UserID">identificador de usuario</param>
        /// <param name="esPemex">validador si es usuario pemex</param>
        /// <returns>vista parcial para el modal</returns>
        [HttpGet]
        public async Task<IActionResult> ModalAdd(Guid? UserID = null, bool esPemex = true)
        {
            try
            {
                // Lista de perfiles
                var perfiles = await _utility.GetItem<DataResult<IEnumerable<ProfilesDto>>>("Catalogos/Profiles");

                // Lista de organismos
                var organismos = await _utility.GetItem<DataResult<IEnumerable<OrganismDto>>>("Catalogos");

                ViewBag.Perfiles = perfiles.Data;
                ViewBag.Organismos = organismos.Data;

                // Crear usuario
                if (UserID == null)
                    return esPemex ? PartialView("Pemex/_ContentModalPemex") : PartialView("External/_ContentExternal");

                // Actualizar usuario
                var responseUser = await _utility.GetItem<DataResult<UsersDto>>($"Usuarios/GetUsuarioByIdEdit/{UserID}");
                if (responseUser.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al procesar la información, por favor intente de nuevo." });

                // Se muestra el modal para pemex o para externo
                return esPemex ? PartialView("Pemex/_ContentModalPemex", responseUser.Data) : PartialView("External/_ContentExternal", responseUser.Data);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Ocurrió un error" });
            }
        }

        /// <summary>
        /// Consulta de usuario en SIO
        /// </summary>
        /// <param name="UserID">ficha del usuario</param>
        /// <returns>Información de usuario</returns>
        [HttpGet]
        public async Task<JsonResult> UsuarioSIO(string Ficha)
        {
            try
            {
                var datos = await _utility.GetItem<DataResult<UsuarioSIODto>>("OldUsuarios/SIO/" + Ficha);

                if (Ficha == "625485")//datos.Data != null)
                {
                    return Json(new UsuarioSIODto()
                    {
                        FichaResult = new UsuarioSIODtoItem()
                        {
                            NOMBRES = "Carlos Augusto",
                            AP_PATERNO = "Corona",
                            AP_MATERNO = "Corona",
                            EMAIL = "carlos.augusto.corona@pemex.com",
                            //CLAVE = "01",
                            RFC_SAT = "COCC820607UH3",
                            DEPTO_CLAVE = "23773",
                            FICHA = "625485",
                            ORG_CLAVE = "CORP"
                        }
                    }); ;
                    //return Json(datos.Data.FichaResult);
                }
                else
                {
                    return Json(new { success = true, responseText = "No se encontraron datos con la ficha ingresada" });
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }

        /// <summary>
        /// Consulta de usuario si existe
        /// </summary>
        /// <param name="UserID">ficha del usuario</param>
        /// <returns>Información de usuario</returns>
        [HttpGet]
        public async Task<bool> ExisteUsuario(string ficha)
        {
            bool result = false;
            return result;

            var datos = await _utility.GetItem<DataResult<IEnumerable<UsuariosSearchResponseDto>>>("OldUsuarios/Search/" + ficha);

            if (datos.Data != null)
            {
                if (datos.Data.ToList().Count > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        [HttpPost]
        public async Task<JsonResult> Agregar(UsuariosPemexInDto pemexInDto, IFormFile? fileID, IFormFile? fileDP, bool esPemex = true)
        {
            try
            {
                pemexInDto.usuarioLogeado = _generals.User.UserType;
                pemexInDto.nombreLogeado = _generals.User.UserName;

                if (esPemex)
                {
                    var previo = await _utility.GetItem<DataResult<UsersDto>>("Usuario/Simple/" + pemexInDto.Ficha);
                    if (previo.Data != null)
                    {
                        if (previo.Data.UserID != Guid.Empty)
                        {
                            return Json(new { success = false, responseText = "Usuario previamente registrado." });
                        }
                    }
                    await _utility.Post<UsuariosPemexInDto>(pemexInDto, "Usuarios/Inserta/Pemex");
                }
                else
                {
                    //var previo = await _utility.GetItem<DataResult<UsersDto>>("Usuarios/Correo/" + pemexInDto.Email);
                    //if (previo.Data != null)
                    //{
                    //    if (previo.Data.UserID != Guid.Empty)
                    //    {
                    //        return Json(new { success = false, responseText = "Correo previamente asignado a otro usuario." });
                    //    }
                    //}

                    if (fileDP != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            fileDP.CopyTo(ms);
                            pemexInDto.FileDP = ms.ToArray();
                            pemexInDto.NameFileDP = fileDP.FileName;
                        }
                    }

                    if (fileID != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            fileID.CopyTo(ms);
                            pemexInDto.FileDI = ms.ToArray();
                            pemexInDto.NameFileDI = fileID.FileName;
                        }
                    }

                    switch (pemexInDto.userType)
                    {
                        case "UserTypeP":
                            await _utility.Post<UsuariosPemexInDto>(pemexInDto, "Usuarios/Inserta/Proveedor");
                            break;
                        case "Auditor":
                            await _utility.Post<UsuariosPemexInDto>(pemexInDto, "Usuarios/Inserta/AuditorPPI");
                            break;
                        case "UsuarioPPI":
                            await _utility.Post<UsuariosPemexInDto>(pemexInDto, "Usuarios/Inserta/AuditorPPI");
                            break;
                    }
                    
                }

                return Json(new { success = true, responseText = "Se agrego Usuario Correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Actualizar(UsuariosPemexInDto pemexInDto, IFormFile? fileID, IFormFile? fileDP, bool esPemex = true)
        {
            try
            {
                pemexInDto.usuarioLogeado = _generals.User.UserType;
                pemexInDto.nombreLogeado = _generals.User.UserName;

                if (esPemex)
                {
                    await _utility.Post<UsuariosPemexInDto>(pemexInDto, "Usuarios/Actualizar/Pemex");
                }
                else
                {
                    if (fileDP != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            fileDP.CopyTo(ms);
                            pemexInDto.FileDP = ms.ToArray();
                            pemexInDto.NameFileDP = fileDP.FileName;
                        }
                    }

                    if (fileID != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            fileID.CopyTo(ms);
                            pemexInDto.FileDI = ms.ToArray();
                            pemexInDto.NameFileDI = fileID.FileName;
                        }
                    }

                    switch (pemexInDto.userType)
                    {
                        case "UserTypeP":
                            await _utility.Post<UsuariosPemexInDto>(pemexInDto, "Usuarios/Actualizar/Proveedor");
                            break;
                        case "Auditor":
                            await _utility.Post<UsuariosPemexInDto>(pemexInDto, "Usuarios/Actualizar/AuditorPPI");
                            break;
                        case "UsuarioPPI":
                            await _utility.Post<UsuariosPemexInDto>(pemexInDto, "Usuarios/Actualizar/AuditorPPI");
                            break;
                    }

                }
                return Json(new { success = true, responseText = "Se agrego Usuario Correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }
    }
}
