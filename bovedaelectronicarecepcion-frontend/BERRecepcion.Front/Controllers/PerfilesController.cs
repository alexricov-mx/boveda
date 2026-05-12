using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Utilities;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class PerfilesController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public PerfilesController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        /// <summary>
        /// Página inicial de Perfiles
        /// </summary>
        /// <returns></returns>
        [RoleFilter(Roles: "AdministrationProfiles")]
        [UserTypeFilter("UserTypeS")]
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Obtiene la lista de perfiles con status 1
        /// </summary>
        /// <param name="pageNum"></param>
        /// <returns>En caso de éxito, devuelve vista parcial con la tabla. En caso de error un Json con un mensaje genérico de error.</returns>
        [HttpGet]
        public async Task<IActionResult> ProfilesTable(int pageNum = 1, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Perfiles").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("search", search));

                var profiles = await _utility.GetItem<DataResult<IEnumerable<ProfilesDto>>>("Perfiles/GetPerfilesAsync", param);

                if (profiles.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    profiles.Pager = new Pager(profiles?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={profiles.Pager.TotalItems}");
                    profiles.Pager = new Pager(profiles.Pager.TotalItems, pageNum, pageSize);
                }
                ViewBag.Search = search;
                return PartialView("_ProfilesTable", profiles);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar los perfiles, por favor intente mas tarde" });
            }
        }
        /// <summary>
        /// Realiza un borrado lógico de un perfil
        /// </summary>
        /// <param name="ProfileID"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Delete(Guid ProfileID)
        {
            try
            {
                var dataResult = new DataResult<ProfilesDto>
                {
                    Data = new ProfilesDto { ProfileID = ProfileID },
                    User = _generals.User
                };
                var result = await _utility.Post(dataResult, "Perfiles/BorraPerfilAsync");
                if (result.Status == System.Net.HttpStatusCode.OK && result.Data.status == false)
                    return Json(new { success = true, message = result.Message });
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al borar el perfil, por favor intente nuevamente." });
            }
        }
        /// <summary>
        /// Devuelve una vista parcial con todos los roles
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CardNew()
        {
            try
            {
                var roles = await _utility.GetItem<DataResult<IEnumerable<RolesCatalogoDto>>>("RolesCatalogo/GetRolesAsync");
                if (roles.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al obener los roles, favor de intentar más tarde." });
                return PartialView("_ContentNewProfile", roles.Data);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar los roles, por favor intente más tarde" });
            }
        }
        /// <summary>
        /// Realiza una inserción de un nuevo perfil
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> InsertaPerfil(ProfilesDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = HandleModelState.GetErrors(ModelState) });

                var dataResult = new DataResult<ProfilesDto>
                {
                    Data = dto,
                    User = _generals.User
                };
                var result = await _utility.Post(dataResult, "Perfiles/InsertaPerfilAsync");
                if (result.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = result.Message });
                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al guardar el perfil, por favor intente más tarde." });
            }
        }
        /// <summary>
        /// Realiza una actualizacion de un perfil
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ActualizaPerfil(ProfilesDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = HandleModelState.GetErrors(ModelState) });
                var dataResult = new DataResult<ProfilesDto>
                {
                    Data = dto,
                    User = _generals.User
                };
                var result = await _utility.Post(dataResult, "Perfiles/ActualizaPerfilAsync");
                if (result.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = result.Message });
                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al guardar el perfil, por favor intente más tarde." });
            }
        }
        /// <summary>
        /// Devuelve un elemento html armado con la información actual del perfil a actualizar
        /// </summary>
        /// <param name="ProfileID"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CardEdit(Guid ProfileID)
        {
            try
            {

                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("ProfileID", ProfileID));
                var perfil = await _utility.GetItem<DataResult<ProfilesDto>>("Perfiles/GetPerfilAsync", param);
                if (perfil.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al obtener el perfil, por favor intente más tarde" });
                return PartialView("_ContentEditProfile", perfil.Data);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener el perfil, por favor intente más tarde" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> DownloadExcel(string search)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("search", search));
                var profiles = await _utility.GetItem<DataResult<IEnumerable<ProfilesDto>>>("Perfiles/GetAllPerfilesAsync", param);
                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Perfiles");
                worksheet.Cell(1, 1).Value = "Name";
                worksheet.Cell(1, 2).Value = "status";
                var perfiles = profiles.Data.ToList();
                for (int index = 1; index <= perfiles.Count; index++)
                {
                    worksheet.Cell(index + 1, 1).Value = perfiles[index - 1].Name;
                    worksheet.Cell(index + 1, 2).Value = perfiles[index - 1].status.ToString();
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Perfiles.xlsx");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return RedirectToAction("Index");
            }
        }
    }
}
