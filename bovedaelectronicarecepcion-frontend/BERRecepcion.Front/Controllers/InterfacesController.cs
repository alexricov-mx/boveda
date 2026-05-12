using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class InterfacesController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public InterfacesController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }

        [RoleFilter(Roles: "AdministrationInterfaces")]
        [UserTypeFilter("UserTypeS")]
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> InterfacesTable()
        {
            try
            {
                var result = await _utility.GetItem<DataResult<IEnumerable<ControlInterfacesDto>>>("Interfaces/GetInterfacesAsync");
                if (result.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al cargar las interfaces de SAP, por favor intente mas tarde" });
                return PartialView("_InterfacesTable", result);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar las interfaces de SAP, por favor intente más tarde" });
            }
        }
        public async Task<IActionResult> ShowModal(Guid sapId, bool? activar = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("sapId", sapId));
                var result = await _utility.GetItem<DataResult<ControlInterfacesDto>>("Interfaces/GetControlInterfacesRolesAsync", param);
                if (result.Status != System.Net.HttpStatusCode.OK || result.Data == null)
                    return Json(new { success = false, message = result.Message });
                ViewBag.Activar = activar;
                return PartialView("_ModalContent", result.Data);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener los roles, por favor intente más tarde" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Actualizar(ControlInterfacesDto dto)
        {
            try
            {
                if (dto == null)
                    return Json(new { success = false, message = "Ocurrió un error al actualizar la información, por favor intente más tarde." });
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = Utilities.HandleModelState.GetErrors(ModelState) });
                bool allSelecter = dto.ControlInterfacesRoles.Where(x => x.status == false).ToList().Count == 0;
                bool somethingSelected = dto.ControlInterfacesRoles.Where(x => x.status == true).ToList().Count > 0;
                bool noneSelected = dto.ControlInterfacesRoles.Where(x => x.status == true).ToList().Count == 0;

                dto.Status = (allSelecter || somethingSelected);
                dto.StatusB = allSelecter ? 1 : somethingSelected ? 2 : noneSelected ? 0 : 0;


                var dataResult = new DataResult<ControlInterfacesDto>
                {
                    Data = dto,
                    User = _generals.User
                };
                var result = await _utility.Post(dataResult, "Interfaces/ActualizarAsync");
                return Json(new { success = result.Status == System.Net.HttpStatusCode.OK, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al actualizar la información, por favor intente más tarde." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ActualizarTodo(ControlInterfacesDto dto, bool activar)
        {
            try
            {
                if (dto == null)
                    return Json(new { success = false, message = "Ocurrió un error al actualizar la información, por favor intente más tarde." });
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = Utilities.HandleModelState.GetErrors(ModelState) });

                dto.Status = activar;
                dto.StatusB = activar ? 1 : 0;
                dto.Activar = activar;

                var dataResult = new DataResult<ControlInterfacesDto>
                {
                    Data = dto,
                    User = _generals.User
                };
                var result = await _utility.Post(dataResult, "Interfaces/ActualizarAsync");
                return Json(new { success = result.Status == System.Net.HttpStatusCode.OK, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al actualizar la información, por favor intente más tarde." });
            }
        }
    }
}
