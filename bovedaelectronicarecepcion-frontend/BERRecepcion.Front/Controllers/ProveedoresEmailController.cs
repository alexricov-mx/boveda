using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    public class ProveedoresEmailController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        protected readonly IGenerals _generals;
        // GET: ProveedoresEmailController

        public ProveedoresEmailController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        
        //[RoleFilter(Roles: "ReportEmails")]
        //[UserTypeFilter("UserTypeP")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> obtenerProveedoresEmail(DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                var ProveedoresEmail = new DataResult<IEnumerable<CopadeDto>>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:ProveedoresEmail").Value);
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("UserID", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", false));
                List<string> cleanedSearchList = new List<string>();
                foreach (var item in search)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                ProveedoresEmail = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>("ProveedoresEmail/GetProveedoresEmailGAsync", param);

                ProveedoresEmail.Pager = new Pager(ProveedoresEmail.Pager.TotalItems, pageNum, pageSize);

                return PartialView("_ProveedoresEmailTable", ProveedoresEmail);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar los proveedoresEmail, por favor intente mas tarde" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelGetAll(DateTime? fechaInicial, DateTime? fechaFinal, string search, int pageNum = 1)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                var ProveedorEmail = new DataResult<IEnumerable<CopadeDto>>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:ProveedoresEmail").Value);
                param.Add(new CustomHttpParameter("fechaInicial", Convert.ToDateTime(fechaInicial).ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", Convert.ToDateTime(fechaFinal).ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("UserID", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", true));
                List<string> cleanedSearchList = new List<string>();
                var searchR = (search??"").Split("__");
                foreach (var item in searchR)
                {
                    string removed = _generals.RemoveSpecialCharacters(item.Replace("_", ","));
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                ProveedorEmail = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>("ProveedoresEmail/GetProveedoresEmailGAsync", param);

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Proveedores Email");
                worksheet.Cell(1, 1).Value = "Organismo";
                worksheet.Cell(1, 2).Value = "Contrato";
                worksheet.Cell(1, 3).Value = "No. Acreedor";
                worksheet.Cell(1, 4).Value = "Nombre del Acreedor";
                worksheet.Cell(1, 5).Value = "Correo envíado a";
                worksheet.Cell(1, 6).Value = "Fecha";
                worksheet.Cell(1, 7).Value = "Estatus";
                worksheet.Cell(1, 8).Value = "Motivo";
                worksheet.Cell(1, 9).Value = "Copade";
                worksheet.Cell(1, 10).Value = "Pedido";
                worksheet.Cell(1, 11).Value = "Tipo de Notificacon";
                //worksheet.Cell(1, 1).Value = "Documento SAP";
                //worksheet.Cell(1, 2).Value = "COPADE";
                //worksheet.Cell(1, 3).Value = "Organismo";
                //worksheet.Cell(1, 4).Value = "Acreedor";
                //worksheet.Cell(1, 5).Value = "Correo"; 
                //worksheet.Cell(1, 6).Value = "Fecha";
                //worksheet.Cell(1, 7).Value = "Tipo Documento";


                int index = 1;
                foreach (var item in ProveedorEmail.Data)
                {
                    worksheet.Cell(index + 1, 1).Value = item.clave;
                    worksheet.Cell(index + 1, 2).Value = item.Contract;
                    worksheet.Cell(index + 1, 3).Value = item.CreditorNumber;
                    worksheet.Cell(index + 1, 4).Value = item.Creditor;
                    worksheet.Cell(index + 1, 5).Value = item.ProviderEmail;
                    worksheet.Cell(index + 1, 6).Value = item.ProviderEmailSendDate;
                    worksheet.Cell(index + 1, 7).Value = item.ProviderEmailSendDate.HasValue ? "Enviado" : "No enviado";
                    worksheet.Cell(index + 1, 8).Value = item.ProviderEmailReason;
                    worksheet.Cell(index + 1, 9).Value = item.Reception;
                    worksheet.Cell(index + 1, 10).Value = item.SapOrder;
                    worksheet.Cell(index + 1, 11).Value = item.TipoDoc;
                    //worksheet.Cell(index + 1, 1).Value = "'" + item.SapOrder;
                    //worksheet.Cell(index + 1, 2).Value = item.Reception;
                    //worksheet.Cell(index + 1, 3).Value = item.clave;
                    //worksheet.Cell(index + 1, 4).Value = item.CreditorNumber;
                    //worksheet.Cell(index + 1, 5).Value = item.ProviderEmail;
                    //worksheet.Cell(index + 1, 6).Value = item.ProviderEmailSendDate;
                    //worksheet.Cell(index + 1, 7).Value = item.TipoDoc;

                    index++;
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProveedoresEmail" + fileName + ".xlsx");
                }
            }
            catch (Exception )
            {
                return RedirectToAction("Index");
            }
        }
    }
}
