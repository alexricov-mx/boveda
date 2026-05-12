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
using System.Text.Json;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class AltaContratosController : Controller
    {
        protected readonly IConfiguration _configuration;
        private readonly ILogger<AltaContratosController> _logger;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;


        public AltaContratosController(IConfiguration configuration, ILogger<AltaContratosController> logger, IRestUtility utility, IGenerals generals)
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
        public async Task<IActionResult> AltaContratosTable(int pageNum = 1, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:AltaContratos").Value);
                param.Add(new CustomHttpParameter("search", search));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                var result = await _utility.GetItem<DataResult<IEnumerable<AltaContratosDto>>>("AltaContratos/ConsultaACAsync", param);
                
                if (result.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    result.Pager = new Pager(result?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={result.Pager.TotalItems}");
                    result.Pager = new Pager(result.Pager.TotalItems, pageNum, pageSize);
                }
                return PartialView("_ACTable", result);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al cargar los contratos, por favor intente mas tarde" });
            }
        }
        public async Task<IActionResult> ShowModal(Guid? AltaContratoID = null, bool isEdit = false)
        {
            try
            {
                DataResult<AltaContratosDto> result = null;
                if (AltaContratoID != null)
                {
                    var param = new List<CustomHttpParameter>();
                    param.Add(new CustomHttpParameter("AltaContratoID", AltaContratoID));
                    result = await _utility.GetItem<DataResult<AltaContratosDto>>("AltaContratos/GetACByIdAsync", param);
                    if (result.Status != System.Net.HttpStatusCode.OK || result.Data == null)
                        return Json(new { Success = false, Message = result.Message });
                }
                
                ViewBag.IsEdit = isEdit;
                return PartialView("_ModalContent", result?.Data);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al cargar la información, por favor intente mas tarde" });
            }
        }
        public async Task<IActionResult> Eliminar(Guid AltaContratoID)
        {
            try
            {
                var dataResult = new DataResult<AltaContratosDto>
                {
                    Data = new AltaContratosDto { AltaContratoID = AltaContratoID },
                    User = _generals.User
                };
                var result = await _utility.Post(dataResult, "AltaContratos/BorraACAsync");
                return Json(new { success = result.Status == System.Net.HttpStatusCode.OK, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al eliminar el contrato, por favor intente más tarde." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Agregar(AltaContratosDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = HandleModelState.GetErrors(ModelState) });
                var dataResult = new DataResult<AltaContratosDto>
                {
                    Data = dto,
                    User = _generals.User
                };
                dto.Usuario_alta = _generals.User.Token;
                var result = await _utility.Post(dataResult, "AltaContratos/InsertaACAsync");
                return Json(new { success = result.Status == System.Net.HttpStatusCode.OK, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al crear un contrato, por favor intente mas tarde" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Editar(AltaContratosDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = HandleModelState.GetErrors(ModelState) });
                var dataResult = new DataResult<AltaContratosDto>
                {
                    Data = dto,
                    User = _generals.User
                };
                dto.Usuario_alta = _generals.User.Token;
                var result = await _utility.Post(dataResult, "AltaContratos/ActualizaACAsync");
                return Json(new { success = result.Status == System.Net.HttpStatusCode.OK, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al actualizar el contrato, por favor intente mas tarde" });
            }
        }

        public async Task<IActionResult> GetAC(int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:AltaContratos").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                var datos = await _utility.GetItem<DataResult<IEnumerable<AltaContratosDto>>>("AltaContratos/ConsultaACGetAllAsync", param);

                datos.Pager = new Pager(datos.Pager.TotalItems, pageNum, pageSize);

                if (datos.Data.Count() <= pageSize)
                {
                    datos.Pager.EndPage = 1;

                }

                return PartialView("_ACTable", datos);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al cargar los contratos, por favor intente mas tarde" });
            }

        }

        

        


        public IActionResult ContentEditAC(AltaContratosDto dto)
        {
            dto.Firma1 = dto.Firma1.Substring(0, 6);
            dto.Firma2 = dto.Firma2.Substring(0, 6);
            dto.Suplente1 = dto.Suplente1.Substring(0, 6);
            dto.Suplente2 = dto.Suplente2.Substring(0, 6);
            return PartialView("_ContentEditAC", dto);
        }

        public IActionResult ContentNewAC(AltaContratosDto dto)
        {
            return PartialView("_ContentNewAC", dto);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelGetAll(int pageNum = 0)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:AltaContratos").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));


                var altacontato = await _utility.GetItem<DataResult<IEnumerable<AltaContratosDto>>>("AltaContratos/ConsultaACGetAllAsync", param);
                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("AltaContrato");
                worksheet.Cell(1, 1).Value = "Contrato";
                worksheet.Cell(1, 2).Value = "Fecha";
                worksheet.Cell(1, 3).Value = "Firma1";
                worksheet.Cell(1, 4).Value = "Suplente1";
                worksheet.Cell(1, 5).Value = "Firma2";
                worksheet.Cell(1, 6).Value = "Suplente2";
                worksheet.Cell(1, 7).Value = "Tipo";

                var ac = altacontato.Data.ToList();
                for (int index = 1; index <= ac.Count; index++)
                {
                    worksheet.Cell(index + 1, 1).Value = ac[index - 1].Contract;
                    worksheet.Cell(index + 1, 2).Value = ac[index - 1].Fecha_Alta;
                    worksheet.Cell(index + 1, 3).Value = ac[index - 1].Firma1;
                    worksheet.Cell(index + 1, 4).Value = ac[index - 1].Suplente1;
                    worksheet.Cell(index + 1, 5).Value = ac[index - 1].Firma2;
                    worksheet.Cell(index + 1, 6).Value = ac[index - 1].Suplente2;
                    //worksheet.Cell(index + 1, 7).Value = ac[index - 1].DocumentType;
                    worksheet.Cell(index + 1, 7).Value = "tipo de documento";
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AltaContratoGetAll" + fileName + ".xlsx");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }
    }
}
