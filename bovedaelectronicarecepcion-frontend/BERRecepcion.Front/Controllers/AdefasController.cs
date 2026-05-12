using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Utilities;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    //[AllowAnonymous]
    public class AdefasController : Controller
    {

        private readonly ILogger<AdefasController> _logger;
        protected readonly IRestUtility _utility;
        protected readonly IConfiguration _configuration;
        protected readonly IGenerals _generals;

        public AdefasController(IConfiguration configuration, ILogger<AdefasController> logger, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _logger = logger;
            _utility = utility;
            _generals = generals;
        }

        [RoleFilter(Roles: "AdministrationAdefa")]
        [UserTypeFilter("UserTypeS")]
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ConsultaAdefas(int pageNum = 1, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Adefas").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("search", search));

                var adefas = await _utility.GetItem<DataResult<IEnumerable<AdefasDto>>>("Adefas", param);
                
                if (adefas.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    adefas.Pager = new Pager(adefas?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={adefas.Pager.TotalItems}");
                    adefas.Pager = new Pager(adefas.Pager.TotalItems, pageNum, pageSize);
                }

                return PartialView("_AdefasTable", adefas);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al cargar la información, por favor intente mas tarde" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> Agregar(AdefasDto adefa)
        {
            try
            {
                if (Convert.ToDateTime(adefa.FinVentana) < Convert.ToDateTime(adefa.InicioVentana))
                    return Json(new { success = false, message = "El inicio de ventana no debe ser mayor al fin de ventana" });
                var dataAdefa = new DataResult<AdefasDto>
                {
                    Data = adefa,
                    User = _generals.User
                };
                var result = await _utility.Post(dataAdefa, "Adefas/InsertaAdefa");
                if (result.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { Success = false, message = result.Message });
                return Json(new { success = true, message = "Periodo de adefa creado con éxito." });
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return Json(new { success = false, message = "Ocurrió un error al realizar la operación, favor de intentar más tarde." });
            }
        }
        [HttpPost]
        public async Task<JsonResult> Editar(AdefasDto adefa)
        {
            try
            {
                if (Convert.ToDateTime(adefa.FinVentana) < Convert.ToDateTime(adefa.InicioVentana))
                    return Json(new { success = false, message = "El inicio de ventana no debe ser mayor al fin de ventana" });
                var dataAdefa = new DataResult<AdefasDto>
                {
                    Data = adefa,
                    User = _generals.User
                };
                var result = await _utility.Post(dataAdefa, "Adefas/ActualizaAdefaAsync");
                if (result.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { Success = false, message = result.Message });
                return Json(new { success = true, message = "Datos Actualizados Correctamente" });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al actualizar el periodo de adefa seleccionado, favor de intentarlo más tarde." });
            }
        }
        public async Task<JsonResult> Eliminar(Guid adefaID)
        {
            try
            {
                DataResult<AdefasDto> adefa = new DataResult<AdefasDto>
                {
                    Data = new AdefasDto { AdefaID = adefaID },
                    User = _generals.User
                };
                var result = await _utility.Post(adefa, "Adefas/BorraAdefaAsync");
                if (result.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { Success = false, message = result.Message });
                return Json(new { success = true, message = "Datos Eliminados Correctamente" });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al eliminar el periodo de adefa, favor de intentar más tarde." });
            }
        }
        public async Task<IActionResult> ShowModal(Guid? adefaID = null, bool isEdit = false)
        {
            try
            {
                var adefa = new DataResult<AdefasDto>();
                if (adefaID != null)    
                {
                    var param = new List<CustomHttpParameter>();
                    param.Add(new CustomHttpParameter("adefaID", adefaID));
                    adefa = await _utility.GetItem<DataResult<AdefasDto>>("Adefas/GetAdefaByIdAsync", param);
                    if (adefa.Status != System.Net.HttpStatusCode.OK || adefa.Data == null)
                        return Json(new { Success = false, Message = adefa.Message });
                }
                int inicio = Convert.ToInt32(_configuration["Adefas:FecNumPasado"]);
                int fin = Convert.ToInt32(_configuration["Adefas:FecNumFuturo"]);
                int startYear = DateTime.Now.AddYears(-inicio).Year;
                int endYear = DateTime.Now.AddYears(fin).Year;
                List<DataDroplist> fechas = new List<DataDroplist>();
                for (int i = startYear; i <= endYear; i++)
                    fechas.Add(new DataDroplist { id = i, nombre = i.ToString() });

                var orgDat = await _utility.GetItem<DataResult<IEnumerable<OrganismDto>>>("Catalogos");
                ViewBag.Organismos = orgDat.Data;
                ViewBag.Fechas = fechas.OrderByDescending(x => x.id).ToList();
                ViewBag.IsEdit = isEdit;
                return PartialView("_ModalContent", adefa?.Data);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al cargar la información, por favor intente mas tarde" });
            }
        }
        public async Task<IActionResult> DownloadExcel(string search)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Adefas").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("search", search));
                param.Add(new CustomHttpParameter("esDescarga", true));

                var adefas = await _utility.GetItem<DataResult<IEnumerable<AdefasDto>>>("Adefas", param);
                if (adefas.Status != System.Net.HttpStatusCode.OK || adefas.Data == null)
                    return Json(new { success = false, message = "Ocurrió un error al cargar la información, por favor intente mas tarde" });
                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Adefas");
                worksheet.Cell(1, 1).Value = "Organismos";
                worksheet.Cell(1, 2).Value = "Año Factura";
                worksheet.Cell(1, 3).Value = "Inicio de Ventana";
                worksheet.Cell(1, 4).Value = "Fin de Ventana";             
                var result = adefas.Data.ToList();
                for (int index = 1; index <= result.Count; index++)
                {
                    worksheet.Cell(index + 1, 1).Value = result[index - 1].OrganismClave;
                    worksheet.Cell(index + 1, 2).Value = result[index - 1].AnhioFactura;
                    worksheet.Cell(index + 1, 3).Value = result[index - 1].InicioVentana;
                    worksheet.Cell(index + 1, 4).Value = result[index - 1].FinVentana;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Adefas " + DateTime.Now.ToShortDateString() + ".xlsx");
                }
            }
            catch (Exception ext)
            {
                return RedirectToAction("Index");
            }
        }
    }
}
