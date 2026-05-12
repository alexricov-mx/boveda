using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class AdmonUserController : Controller
    {
        private readonly ILogger<AdmonUserController> _logger;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public AdmonUserController(ILogger<AdmonUserController> logger, IRestUtility utility, IGenerals generals)
        {
            _logger = logger;
            _utility = utility;
            _generals = generals;
        }

        [RoleFilter(Roles: "AdministrationUsers")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public async Task<IActionResult> Index()
        {
            try
            {
                //Muestra todos los perfiles
                var perfilesDat = await _utility.GetItem<DataResult<IEnumerable<ProfilesDto>>>("Catalogos/Profiles");
                ViewBag.ddPerfilesI = new SelectList(perfilesDat.Data.ToList(), "ProfileID", "Name");
                ViewBag.ddPerfilesP = new SelectList(perfilesDat.Data.ToList(), "ProfileID", "Name");

                //Muestra la opciones para tipos de usuarios proveedores
                List<DataDroplist> fec = new List<DataDroplist>();

                fec.Add(new DataDroplist() { id = 0, nombre = "Proveedor" });
                fec.Add(new DataDroplist() { id = 1, nombre = "Auditor" });
                fec.Add(new DataDroplist() { id = 2, nombre = "UsuarioPPI" });
                ViewBag.ddUsuarios = new SelectList(fec.ToList(), "id", "nombre");

                //Muestra todos los organismos
                var orgDat = await _utility.GetItem<DataResult<IEnumerable<OrganismDto>>>("Catalogos");
                ViewBag.ddOrganismos = new SelectList(orgDat.Data.ToList(), "OrganismID", "Name");
                IEnumerable<OrganismDto> organisms = orgDat.Data;

                //muestra el calendario de inicio y fin
                var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
                var totalFuturo = Convert.ToInt32(config["AdmonUsuarios:FecNumFuturo"]);
                var totalPasado = Convert.ToInt32(config["AdmonUsuarios:FecNumPasado"]);
                var total = Convert.ToInt32(config["FecNum"]);
                ViewBag.ddFecha = new SelectList(fec.ToList(), "id", "nombre");
                return View(organisms);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> UserPemex()
        {
            return await Task.Run(() => PartialView("_UserPemex"));
        }
        [HttpGet]
        public async Task<IActionResult> UserExternal()
        {
            return await Task.Run(() => PartialView("_UserExternal"));
        }

        [HttpGet]
        public async Task<IActionResult> UserTable(string search = null)
        {
            try
            {
                var response = await _utility.GetItem<DataResult<IEnumerable<UsuariosSearchResponseDto>>>("OldUsuarios/Search/" + search);
                return PartialView("_UserTable", response);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Ocurrió un error" });
            }
        }

















        public async Task<IActionResult> Buscar(string texto)
        {
            try
            {
                var datos = await _utility.GetItem<DataResult<IEnumerable<UsuariosSearchResponseDto>>>("OldUsuarios/Search/" + texto);

                //para poder pintar controles que son especificos de un super usuario.
                if (_generals.User.UserType.Equals("UserTypeS") && datos.Data.Count() != 0)
                    datos.Data.FirstOrDefault().IsSuperUsuario = true;

                return PartialView("Views/AdmonUser/DatosUsuario.cshtml", datos);

            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return PartialView(exp.Message);
            }
        }
        public async Task<IActionResult> UsuarioAcreedor(string creditorNumber)
        {
            string UserName = _generals.User.UserType;

            try
            {

                var datos = await _utility.GetItem<DataResult<IEnumerable<UsuariosNaResponseDto>>>("OldUsuarios/Acreedores/" + UserName + "/" + creditorNumber);

                return PartialView(datos);
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return PartialView(exp.Message);
            }
        }
        public async Task<IActionResult> UsuarioAuditorPpi(string Correo)
        {
            string UserName = _generals.User.UserType;
            
            try
            {

                var datos = await _utility.GetItem<DataResult<UsersDto>>("OldUsuarios/Correo/" + UserName + "/" + Correo);

                if (datos.Data.UserType == "UserTypeS" || datos.Data.UserType == "UserTypeA" || datos.Data.UserType == "UserTypeF")
                {
                    var datosFicha = await _utility.GetItem<DataResult<UsuariosFichaResponseDto>>("OldUsuarios/Ficha/" + UserName + "/" + datos.Data.Token);
                    return PartialView("FichasUsuario", datosFicha);
                }
                else if (datos.Data.UserType == "UserTypeP")
                {
                    var datosAcreedor = await _utility.GetItem<DataResult<IEnumerable<UsuariosNaResponseDto>>>("OldUsuarios/Acreedor/" + UserName + "/" + datos.Data.UserID);
                    return PartialView("UsuarioAcreedor", datosAcreedor);
                }
                else
                {
                    return PartialView("UsuarioAuditorPpi", datos);
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return PartialView("Ocurrió un error al procesar la información, por favor intente de nuevo.En caso de persistir el error, por favor contacte a su administrador.");
            }
        }
        public async Task<JsonResult> UsuarioSIO(string Ficha)
        {
            try
            {
                var datos = await _utility.GetItem<DataResult<UsuarioSIODto>>("OldUsuarios/SIO/" + Ficha);

                if (datos.Data != null)
                {
                    return Json(datos.Data.FichaResult);
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
        public async Task<JsonResult> Agregar(UsuariosPemexInDto pemexInDto)
        {
            pemexInDto.usuarioLogeado = _generals.User.UserType;
            pemexInDto.nombreLogeado = _generals.User.UserName;

            try
            {
                var previo = await _utility.GetItem<DataResult<UsersDto>>("OldUsuarios/Correo/" + pemexInDto.Email);
                if(previo.Data!=null)
                {
                    if (previo.Data.UserID != Guid.Empty)
                    {
                        return Json(new { success = false, responseText = "Correo previamente asignado a otro usuario." });
                    }
                }

                var datos = await _utility.Post<UsuariosPemexInDto>(pemexInDto, "Usuarios/Inserta/Pemex");
                return Json(new { success = true, responseText = "Se agrego Usuario Correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }
        public async Task<JsonResult> AgregarP(UsuariosPemexInDto proveedorInDto)
        {
            proveedorInDto.usuarioLogeado = _generals.User.UserType;
            proveedorInDto.nombreLogeado = _generals.User.UserName;

            try
            {
                var previo = await _utility.GetItem<DataResult<UsersDto>>("OldUsuarios/Correo/" + proveedorInDto.Email);
                if (previo.Data != null)
                {
                    if (previo.Data.UserID != Guid.Empty)
                    {
                        return Json(new { success = false, responseText = "Correo previamente asignado a otro usuario." });
                    }
                }

                var datos = await _utility.Post<UsuariosPemexInDto>(proveedorInDto, "Usuarios/Inserta/Proveedor");

                return Json(new { success = true, responseText = "Se agrego Usuario Correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }
        public async Task<JsonResult> AgregarAuditorPpi(UsuariosPemexInDto auditorPpiDto)
        {
            auditorPpiDto.usuarioLogeado = _generals.User.UserType;
            auditorPpiDto.nombreLogeado = _generals.User.UserName;

            try
            {
                var previo = await _utility.GetItem<DataResult<UsersDto>>("OldUsuarios/Correo/" + auditorPpiDto.Email);
                if (previo.Data != null)
                {
                    if (previo.Data.UserID != Guid.Empty)
                    {
                        return Json(new { success = false, responseText = "Correo previamente asignado a otro usuario." });
                    }
                }

                var datos = await _utility.Post<UsuariosPemexInDto>(auditorPpiDto, "Usuarios/Inserta/AuditorPPI");

                return Json(new { success = true, responseText = "Se agrego Usuario Correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }
        public async Task<JsonResult> Editar(UsuariosPemexInDto pemexDto)
        {
            pemexDto.usuarioLogeado = _generals.User.UserType;
            pemexDto.nombreLogeado = _generals.User.UserName;
            
            try
            {
                var previo = await _utility.GetItem<DataResult<UsersDto>>("OldUsuarios/Correo/" + pemexDto.Email);
                if (previo.Data != null)
                {
                    if (previo.Data.UserID != Guid.Empty)
                    {
                        if (previo.Data.UserID != pemexDto.userId)
                        {
                            return Json(new { success = false, responseText = "Correo previamente asignado a otro usuario." });
                        }
                    }
                }

                var datos = await _utility.Post<UsuariosPemexInDto>(pemexDto, "Usuarios/Actualiza/Pemex");
                return Json(new { success = true, responseText = "Se Actualizo Usuario Correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }
        public async Task<JsonResult> EditarP(UsuariosPemexInDto proveedorDto)
        {
            proveedorDto.usuarioLogeado = _generals.User.UserType;
            proveedorDto.nombreLogeado = _generals.User.UserName;
            
            try
            {
                var previo = await _utility.GetItem<DataResult<UsersDto>>("OldUsuarios/Correo/" + proveedorDto.Email);
                if (previo != null)
                {
                    if (previo.Data.UserID != Guid.Empty)
                    {
                        if (previo.Data.UserID != proveedorDto.userId)
                        {
                            return Json(new { success = false, responseText = "Correo previamente asignado a otro usuario." });
                        }
                    }
                }

                var datos = await _utility.Post<UsuariosPemexInDto>(proveedorDto, "Usuarios/Actualiza/Proveedor");
                return Json(new { success = true, responseText = "Se Actualizo Usuario Correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }
        public async Task<JsonResult> EditarAuditorPpi(UsuariosPemexInDto auditorPpiDto)
        {
            auditorPpiDto.usuarioLogeado = _generals.User.UserType;
            auditorPpiDto.nombreLogeado = _generals.User.UserName;
            
            try
            {
                var previo = await _utility.GetItem<DataResult<UsersDto>>("OldUsuarios/Correo/" + auditorPpiDto.Email);
                if (previo != null)
                {
                    if (previo.Data.UserID != Guid.Empty)
                    {
                        if (previo.Data.UserID != auditorPpiDto.userId)
                        {
                            return Json(new { success = false, responseText = "Correo previamente asignado a otro usuario." });
                        }
                    }
                }

                var datos = await _utility.Post<UsuariosPemexInDto>(auditorPpiDto, "Usuarios/Actualiza/AuditorPPIAsync");
                return Json(new { success = true, responseText = "Se Actualizo Usuario Correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }
        public async Task<JsonResult> StatusUsuarioPemex(string UserName, string Name, string Email, Guid userId)
        {

            string UserNameModifier = _generals.User.UserType;
            
            try
            {

                var datos = await _utility.GetItem<DataResult<UsuariosFichaResponseDto>>("OldUsuarios/Estatus/" + UserNameModifier + "/" + UserName + "/" + Name + "/" + Email + "/" + userId);
                return Json(new { success = true, responseText = "Se agrego Usuario Correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }
        public async Task<JsonResult> SuperUsuario(Guid userId)
        {
            string UserName = _generals.User.UserType;
            
            try
            {

                var datos = await _utility.GetItem<DataResult<UsuariosFichaResponseDto>>("OldUsuarios/Administra/pemex/" + UserName + "/" + userId);
                return Json(new { success = true, responseText = "Se cambio el tipo de usuario correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }
        public async Task<JsonResult> AdminUsuario(Guid userId)
        {
            string UserName = _generals.User.UserType;
            
            try
            {

                var datos = await _utility.GetItem<DataResult<UsuariosFichaResponseDto>>("OldUsuarios/Administra/" + UserName + "/" + userId);
                return Json(new { success = true, responseText = "Se cambio el tipo de usuario correctamente" });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, responseText = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }

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
    }
}
