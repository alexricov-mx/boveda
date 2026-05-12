using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class FirmaAlternosCOPADEController : Controller
    {
        protected readonly IConfiguration _configuration;
        private readonly ILogger<FirmaAlternosCOPADEController> _logger;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;


        public FirmaAlternosCOPADEController(IConfiguration configuration, ILogger<FirmaAlternosCOPADEController> logger, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _logger = logger;
            _utility = utility;
            _generals = generals;
        }

        [RoleFilter(Roles: "ContractsRegister")]
        [UserTypeFilter("UserTypeS,UserTypeA")]

        public ActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> BusquedaAC(string altacontrato = null)
        {
            try
            {
                if (altacontrato != null)
                {
                    var datos = await _utility.GetItem<DataResult<IEnumerable<AltaContratosDto>>>("AltaContratos/" + altacontrato);
                    ViewBag.elcontrato = altacontrato;

                    return PartialView("_ACTable", datos);
                }
                else
                {
                    return RedirectToAction("GetAC");
                }
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al cargar los contratos, por favor intente mas tarde" });
            }

        }
        public async Task<IActionResult> GetAC()
        {
            try
            {
                var datos = await _utility.GetItem<DataResult<IEnumerable<AltaContratosDto>>>("AltaContratos/ConsultaACGetAllAsync");

                return PartialView("_ACTable", datos);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al cargar los contratos, por favor intente mas tarde" });
            }

        }

        [HttpPost]
        public async Task<IActionResult> NuevoAC(AltaContratosDto dto)
        {
            try
            {
                //if (ModelState.IsValid)
                //{
                var dataResult = new DataResult<AltaContratosDto>
                {
                    Data = dto,
                    User = _generals.User
                    //User = new UsersDto {UserID = Guid.Parse("8B079B25-5528-4737-BB00-295858144823") } 
                };
                dto.Usuario_alta = _generals.User.Token;
                //dto.Usuario_alta = "445626";
                var result = await _utility.Post(dataResult, "AltaContratos/InsertaACAsync");
                if (result.Status == System.Net.HttpStatusCode.OK)
                    return Json(new { Success = true, Message = result.Message });
                else
                    return Json(new { Success = false, Message = result.Message });
                //}
                return Json(new { Success = false, Message = HandleModelState.GetErrors(ModelState) });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al crear un contrato, por favor intente mas tarde" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarAC(AltaContratosDto dto)
        {
            try
            {
                //if (ModelState.IsValid)
                //{
                var dataResult = new DataResult<AltaContratosDto>
                {
                    Data = dto,
                    User = _generals.User
                    //User = new UsersDto { UserID = Guid.Parse("8B079B25-5528-4737-BB00-295858144823") }
                };
                Guid id = (Guid)dto.AltaContratoID;
                dto.Usuario_alta = _generals.User.Token;
                //dto.Usuario_alta = "445626";
                var result = await _utility.Update(dataResult, id, "AltaContratos/ActualizaACAsync");
                if (result.Status == System.Net.HttpStatusCode.OK)
                    return Json(new { Success = true, Message = result.Message });
                else
                    return Json(new { Success = false, Message = result.Message });
                //}
                return Json(new { Success = false, Message = HandleModelState.GetErrors(ModelState) });
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al actualizar el contrato, por favor intente mas tarde" });
            }
        }

        public async Task<IActionResult> DeleteAC(Guid acId, string elcontrato)
        {
            try
            {
                var dataResult = new DataResult<AltaContratosDto>
                {
                    Data = new AltaContratosDto { AltaContratoID = acId, Contract = elcontrato },
                    User = _generals.User
                    //User = new UsersDto { UserID = Guid.Parse("8B079B25-5528-4737-BB00-295858144823") }
                };
                //var result = await _utility.Delete<DataResult<AltaContratosDto>>(acId, "AltaContratos/BorraACAsync");
                var result = await _utility.Update(dataResult, acId, "AltaContratos/BorraACAsync");
                if (result.Status == System.Net.HttpStatusCode.OK)
                    return Json(new { Success = true, Message = result.Message });
                return Json(new { Success = false, Message = result.Message });
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al eliminar el contrato, por favor intente mas tarde" });
            }
        }

        public IActionResult ContentEditAC(AltaContratosDto dto)
        {
            return PartialView("_ContentEditAC", dto);
        }

        public IActionResult ContentNewAC(AltaContratosDto dto)
        {
            return PartialView("_ContentNewAC", dto);
        }

    }
}
