using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
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
    [ValidateUser]

    public class ConsultasController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        protected readonly IGenerals _generals;
        private readonly IHostEnvironment _env;

        public ConsultasController(IConfiguration configuration, IRestUtility utility, IGenerals generals, IHostEnvironment env)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
            _env = env;
        }

        #region RechazosAnalitico

        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        [RoleFilter(Roles: "ReportRejectedInvoiceAnalytic")]
        //[UserTypeFilter("UserTypeP")]
        public IActionResult RechazosAnalitico()
        {
            return View("~/Views/Consultas/RechazosAnalitico/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> obtenerRechazosAnaliticoTable(DateTime start, DateTime end, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var RechazosSeguimiento = new DataResult<IEnumerable<ReporteRechazosDto>>();

                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("source", 1));      // 0 - factura, 1 - analitico

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", false));
                List<string> cleanedSearchList = new List<string>();
                foreach (var item in search)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                RechazosSeguimiento = await _utility.GetItem<DataResult<IEnumerable<ReporteRechazosDto>>>("Consultas/GetListaRechazosAsync", param);

                RechazosSeguimiento.Pager = new Pager(RechazosSeguimiento.Data.ToList().Count(), pageNum, pageSize);
                if (RechazosSeguimiento.Data.ToList().Count() <= pageSize)
                {
                    RechazosSeguimiento.Pager.EndPage = 1;

                }
                RechazosSeguimiento.Pager.CurrentPage = pageNum;
                RechazosSeguimiento.Data = RechazosSeguimiento.Data.Skip((pageNum - 1) * pageSize).Take(pageSize);
                return PartialView("~/Views/Consultas/RechazosAnalitico/GenerarTablaRechazos.cshtml", RechazosSeguimiento);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar los rechazos, por favor intente mas tarde" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelGetAllRechazosAnalitico(DateTime start, DateTime end, string search, int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var RechazosSeguimiento = new DataResult<IEnumerable<ReporteRechazosDto>>();

                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("source", 1));      // 0 - factura, 1 - analitico

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", true));
                List<string> cleanedSearchList = new List<string>();
                var searchR = (search ?? "").Split("__");
                foreach (var item in searchR)
                {
                    string removed = _generals.RemoveSpecialCharacters(item.Replace("_", ","));
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                RechazosSeguimiento = await _utility.GetItem<DataResult<IEnumerable<ReporteRechazosDto>>>("Consultas/GetListaRechazosAsync", param);

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("FacturasAnalitico Rechazadas");
                worksheet.Cell(1, 1).Value = "Documento";
                worksheet.Cell(1, 2).Value = "RFC Acreedor";
                worksheet.Cell(1, 3).Value = "Razon Social";
                worksheet.Cell(1, 4).Value = "RFC Receptor";
                worksheet.Cell(1, 5).Value = "Organismo";
                worksheet.Cell(1, 6).Value = "Fecha Rechazo";
                worksheet.Cell(1, 7).Value = "Correo";
                worksheet.Cell(1, 8).Value = "Motivo";

                int index = 1;
                foreach (var item in RechazosSeguimiento.Data)
                {
                    foreach (var itemMotivo in item.MotivoRechazo)
                    {
                        worksheet.Cell(index + 1, 1).Value = "'" + item.Documento;
                        worksheet.Cell(index + 1, 2).Value = item.RFCAcreedor;
                        worksheet.Cell(index + 1, 3).Value = item.RazonSocial;
                        worksheet.Cell(index + 1, 4).Value = item.RFCReceptor;
                        worksheet.Cell(index + 1, 5).Value = item.Organismo;
                        worksheet.Cell(index + 1, 6).Value = item.FechaRechazo.ToString("dd/MM/yyyy hh:mm");
                        worksheet.Cell(index + 1, 7).Value = item.Correo;
                        worksheet.Cell(index + 1, 8).Value = itemMotivo;
                        index++;
                    }
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RechazosFacturaAnalitico" + fileName + ".xlsx");
                }
            }
            catch (Exception )
            {
                return RedirectToAction("RechazosAnalitico");
            }
        }

        #endregion

        #region RechazosFactura

        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        [RoleFilter(Roles: "ReportRejectedInvoice")]
        //[UserTypeFilter("UserTypeP")]
        public IActionResult RechazosFactura()
        {
            return View("~/Views/Consultas/RechazosFactura/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> obtenerRechazosFacturaTable(DateTime start, DateTime end, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var RechazosSeguimiento = new DataResult<IEnumerable<ReporteRechazosDto>>();

                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("source", 0));      // 0 - factura, 1 - analitico

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", false));
                List<string> cleanedSearchList = new List<string>();
                foreach (var item in search)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                RechazosSeguimiento = await _utility.GetItem<DataResult<IEnumerable<ReporteRechazosDto>>>("Consultas/GetListaRechazosAsync", param);

                RechazosSeguimiento.Pager = new Pager(RechazosSeguimiento.Data.ToList().Count(), pageNum, pageSize);
                if (RechazosSeguimiento.Data.ToList().Count() <= pageSize)
                {
                    RechazosSeguimiento.Pager.EndPage = 1;

                }
                RechazosSeguimiento.Pager.CurrentPage = pageNum;
                RechazosSeguimiento.Data = RechazosSeguimiento.Data.Skip((pageNum - 1) * pageSize).Take(pageSize);
                return PartialView("~/Views/Consultas/RechazosFactura/GenerarTablaRechazos.cshtml", RechazosSeguimiento);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar los rechazos, por favor intente mas tarde" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelGetAllRechazosFactura(DateTime start, DateTime end, string search, int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var RechazosSeguimiento = new DataResult<IEnumerable<ReporteRechazosDto>>();

                int pageSize = 10000;
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("source", 0));      // 0 - factura, 1 - analitico

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", true));
                List<string> cleanedSearchList = new List<string>();
                var searchR = (search ?? "").Split("__");
                foreach (var item in searchR)
                {
                    string removed = _generals.RemoveSpecialCharacters(item.Replace("_", ","));
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                RechazosSeguimiento = await _utility.GetItem<DataResult<IEnumerable<ReporteRechazosDto>>>("Consultas/GetListaRechazosAsync", param);

                RechazosSeguimiento.Pager = new Pager(RechazosSeguimiento.Data.ToList().Count(), pageNum, pageSize);
                if (RechazosSeguimiento.Data.ToList().Count() <= pageSize)
                {
                    RechazosSeguimiento.Pager.EndPage = 1;

                }

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Facturas Rechazadas");
                worksheet.Cell(1, 1).Value = "Documento";
                worksheet.Cell(1, 2).Value = "RFC Acreedor";
                worksheet.Cell(1, 3).Value = "Razon Social";
                worksheet.Cell(1, 4).Value = "RFC Receptor";
                worksheet.Cell(1, 5).Value = "Organismo";
                worksheet.Cell(1, 6).Value = "Fecha Rechazo";
                worksheet.Cell(1, 7).Value = "Correo";
                worksheet.Cell(1, 8).Value = "Motivo";

                int index = 1;
                foreach (var item in RechazosSeguimiento.Data)
                {
                    foreach (var itemMotivo in item.MotivoRechazo)
                    {
                        worksheet.Cell(index + 1, 1).Value = "'" + item.Documento;
                        worksheet.Cell(index + 1, 2).Value = item.RFCAcreedor;
                        worksheet.Cell(index + 1, 3).Value = item.RazonSocial;
                        worksheet.Cell(index + 1, 4).Value = item.RFCReceptor;
                        worksheet.Cell(index + 1, 5).Value = item.Organismo;
                        worksheet.Cell(index + 1, 6).Value = item.FechaRechazo.ToString("dd/MM/yyyy hh:mm");
                        worksheet.Cell(index + 1, 7).Value = item.Correo;
                        worksheet.Cell(index + 1, 8).Value = itemMotivo;
                        index++;
                    }
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RechazosFactura" + fileName + ".xlsx");
                }
            }
            catch (Exception )
            {
                return RedirectToAction("RechazosFactura");
            }
        }

        #endregion

        #region copade Bancario

        [RoleFilter(Roles: "StatisticsCopadeBanking")]
        [UserTypeFilter("UserTypeP")]
        public IActionResult CopadeBancario()
        {
            return View("~/Views/Consultas/CopadeBancario/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> obtenerCOPADETable(DateTime start, DateTime end, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var CopadeSeguimiento = new DataResult<IEnumerable<CopadeDto>>();

                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
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

                var org = _generals.User.Organisms.FirstOrDefault();
                string claveOrganismo = "";
                if (org != null) claveOrganismo = org.Clave;
                param.Add(new CustomHttpParameter("claveOrganismo", claveOrganismo));
                param.Add(new CustomHttpParameter("creditorNumber", _generals.User.CreditorNumber));
                CopadeSeguimiento = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>("Consultas/GetListaCopadeBancarioAsync", param);
                
                if (CopadeSeguimiento.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    CopadeSeguimiento.Pager = new Pager(CopadeSeguimiento?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={CopadeSeguimiento.Pager.TotalItems}");
                    CopadeSeguimiento.Pager = new Pager(CopadeSeguimiento.Pager.TotalItems, pageNum, pageSize);
                }

                return PartialView("~/Views/Consultas/CopadeBancario/GenerarTablaCopade.cshtml", CopadeSeguimiento);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar los copades, por favor intente mas tarde" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelGetAllCOPADE(DateTime start, DateTime end, string search, int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var CopadeSeguimiento = new DataResult<IEnumerable<CopadeDto>>();

                param.Add(new CustomHttpParameter("pageSize", 10000));
                param.Add(new CustomHttpParameter("pageNum", pageNum == 0 ? 1 : pageNum));
                param.Add(new CustomHttpParameter("UserID", _generals.User.UserID));

                var org = _generals.User.Organisms.FirstOrDefault();
                string claveOrganismo = "";
                if (org != null) claveOrganismo = org.Clave;
                param.Add(new CustomHttpParameter("claveOrganismo", claveOrganismo));

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("UserID", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", true));
                List<string> cleanedSearchList = new List<string>();
                var searchR = search.Split("__");
                foreach (var item in searchR)
                {
                    string removed = _generals.RemoveSpecialCharacters(item.Replace("_", ","));
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                CopadeSeguimiento = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>("Consultas/GetListaCopadeBancarioAsync", param);

                int pageSize;
                if (CopadeSeguimiento.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, usando count de datos");
                    pageSize = CopadeSeguimiento?.Data?.Count() ?? 0;
                    CopadeSeguimiento.Pager = new Pager(pageSize, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={CopadeSeguimiento.Pager.TotalItems}");
                    pageSize = CopadeSeguimiento.Pager.TotalItems;
                    CopadeSeguimiento.Pager = new Pager(CopadeSeguimiento.Pager.TotalItems, pageNum, pageSize);
                }
                
                if (CopadeSeguimiento.Pager.TotalItems <= pageSize)
                {
                    CopadeSeguimiento.Pager.EndPage = 1;
                }

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("COPADE Bancario");
                worksheet.Cell(1, 1).Value = "COPADE";
                worksheet.Cell(1, 2).Value = "No. OS";
                worksheet.Cell(1, 3).Value = "No. Contrato";
                worksheet.Cell(1, 4).Value = "Organismo";
                worksheet.Cell(1, 5).Value = "Tipo";
                worksheet.Cell(1, 6).Value = "Centro";
                worksheet.Cell(1, 7).Value = "Fecha Recepcion";
                worksheet.Cell(1, 8).Value = "Ejercicio";
                worksheet.Cell(1, 9).Value = "Total";
                worksheet.Cell(1, 10).Value = "Firmante 1";
                worksheet.Cell(1, 11).Value = "Firmante 2";

                int index = 1;
                foreach (var item in CopadeSeguimiento.Data)
                {
                    worksheet.Cell(index + 1, 1).Value = "'" + item.Reception;
                    worksheet.Cell(index + 1, 2).Value = "'" + item.SapOrder;
                    worksheet.Cell(index + 1, 3).Value = "'" + item.Contract;
                    worksheet.Cell(index + 1, 4).Value = item.clave;
                    worksheet.Cell(index + 1, 5).Value = item.DocumentType;
                    worksheet.Cell(index + 1, 6).Value = item.Center;
                    //worksheet.Cell(index + 1, 7).Value = item.ReceptionDate.HasValue ?? item.ReceptionDate.ToString("dd/MM/yyyy hh:mm");
                    worksheet.Cell(index + 1, 7).Value = item.ReceptionDate.ToString();
                    worksheet.Cell(index + 1, 8).Value = item.Exercise;
                    worksheet.Cell(index + 1, 9).Value = item.Total;
                    worksheet.Cell(index + 1, 10).Value = string.IsNullOrWhiteSpace(item.Functionary1SignDate.ToString()) ? "" : "Firmado";
                    worksheet.Cell(index + 1, 11).Value = string.IsNullOrWhiteSpace(item.Functionary2SignDate.ToString()) ? "" : "Firmado";
                    index++;
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CopadeBancario" + fileName + ".xlsx");
                }
            }
            catch (Exception )
            {
                return RedirectToAction("COPADE");
            }
        }

        [HttpPost]
        public async Task<string> pdfPreviewCOPADE(PICopadeRequestDto dto)
        {
            DtoPdfPreviewData dtoPreview = new DtoPdfPreviewData();
            dtoPreview.data = dto;

            var dataResult = new DataResult<DtoPdfPreviewData>
            {
                Data = dtoPreview,
                User = _generals.User
            };
            dtoPreview.UsuarioModificador = _generals.User.Token;
            try
            {
                if (dto.Functionary1SignDate)  // Para identificar sí es primer firma o segunda firma
                {
                    // aqui recuperamos el PDF  - segunda firma
                    // recuperar el paqueteId y documentoId
                    var param = new List<CustomHttpParameter>();
                    param.Add(new CustomHttpParameter("DocumentoBEId", dto.CopadeId));
                    var documentoFirmado = await _utility.GetItem<DataResult<DocumentoFirmadoDto>>("DocumentoFirmado/GetDocumentoFirmadoAsync", param);
                    if (documentoFirmado.Data == null)
                        return "";

                    var resultRecuperado = await _utility.GetItem<DataResult<ArchivoPDFDto>>($"ESign/RecuperaPDFFirmadoAsync/{documentoFirmado.Data.paqueteId}/{documentoFirmado.Data.documentoId}");
                    if (resultRecuperado.Status == System.Net.HttpStatusCode.OK)
                        return resultRecuperado.Data.ARCHIVO;
                    else
                        return "";
                }
                else
                {
                    // armamos nuestro PDF - primer firma
                    var result = await _utility.Post<DataResult<DtoPdfPreviewData>>(dataResult, "Copade");
                    if (result.Status == System.Net.HttpStatusCode.OK)
                        return result.Data.result.FirstOrDefault();
                    else
                        return "";
                }
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }

            return "";
        }

        #endregion

        #region Estimaciones Bancarias

        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        [RoleFilter(Roles: "StatisticsBankEstimate")]
        //[UserTypeFilter("UserTypeP")]
        public IActionResult EstimacionesBancarias()
        {
            return View("~/Views/Consultas/EstimacionesBancarias/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> obtenerEstimacionesBancariasTable(DateTime start, DateTime end, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var EstimacionSeguimiento = new DataResult<IEnumerable<SOEstimationDto>>();

                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
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

                var org = _generals.User.Organisms.FirstOrDefault();
                string claveOrganismo = "";
                if (org != null) claveOrganismo = org.Clave;
                param.Add(new CustomHttpParameter("claveOrganismo", claveOrganismo));
                EstimacionSeguimiento = await _utility.GetItem<DataResult<IEnumerable<SOEstimationDto>>>("Consultas/GetListaEstimacionesBancariasAsync", param);
                
                if (EstimacionSeguimiento.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    EstimacionSeguimiento.Pager = new Pager(EstimacionSeguimiento?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={EstimacionSeguimiento.Pager.TotalItems}");
                    EstimacionSeguimiento.Pager = new Pager(EstimacionSeguimiento.Pager.TotalItems, pageNum, pageSize);
                }

                return PartialView("~/Views/Consultas/EstimacionesBancarias/GenerarTablaEstimacion.cshtml", EstimacionSeguimiento);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar las estimaciones, por favor intente mas tarde" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelGetAllEstimacionesBancarias(DateTime start, DateTime end, string filtro, int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var EstimacionSeguimiento = new DataResult<IEnumerable<SOEstimationDto>>();

                param.Add(new CustomHttpParameter("pageSize", 10000));
                param.Add(new CustomHttpParameter("pageNum", pageNum == 0 ? 1 : pageNum));
                param.Add(new CustomHttpParameter("UserID", _generals.User.UserID));
                var org = _generals.User.Organisms.FirstOrDefault();
                string claveOrganismo = "";
                if (org != null) claveOrganismo = org.Clave;
                param.Add(new CustomHttpParameter("claveOrganismo", claveOrganismo));

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                if (filtro == null) filtro = "";
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("Filtro", filtro));
                EstimacionSeguimiento = await _utility.GetItem<DataResult<IEnumerable<SOEstimationDto>>>("Consultas/GetListaEstimacionesBancariasAsync", param);

                int pageSize;
                if (EstimacionSeguimiento.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, usando count de datos");
                    pageSize = EstimacionSeguimiento?.Data?.Count() ?? 0;
                    EstimacionSeguimiento.Pager = new Pager(pageSize, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={EstimacionSeguimiento.Pager.TotalItems}");
                    pageSize = EstimacionSeguimiento.Pager.TotalItems;
                    EstimacionSeguimiento.Pager = new Pager(EstimacionSeguimiento.Pager.TotalItems, pageNum, pageSize);
                }
                
                if (EstimacionSeguimiento.Pager.TotalItems <= pageSize)
                {
                    EstimacionSeguimiento.Pager.EndPage = 1;

                }

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Estimaciones Bancarias");
                worksheet.Cell(1, 1).Value = "No. OS";
                worksheet.Cell(1, 2).Value = "No. Contrato";
                worksheet.Cell(1, 3).Value = "Organismo";
                worksheet.Cell(1, 4).Value = "Tipo";
                worksheet.Cell(1, 5).Value = "Acreedor";
                worksheet.Cell(1, 6).Value = "Total";
                worksheet.Cell(1, 7).Value = "Moneda";

                int index = 1;
                foreach (var item in EstimacionSeguimiento.Data)
                {
                    worksheet.Cell(index + 1, 1).Value = "'" + item.SapOrder;
                    worksheet.Cell(index + 1, 2).Value = "'" + item.Contract;
                    worksheet.Cell(index + 1, 3).Value = item.OrganismClave;
                    worksheet.Cell(index + 1, 4).Value = item.DocumentType == "M" ? "Medicamentos" : "Obras";
                    worksheet.Cell(index + 1, 5).Value = item.CreditorNumber;
                    worksheet.Cell(index + 1, 6).Value = item.Total;
                    worksheet.Cell(index + 1, 7).Value = item.Currency;
                    index++;
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EstimacionesBancarias" + fileName + ".xlsx");
                }
            }
            catch (Exception )
            {
                return RedirectToAction("EstimacionesBancarias");
            }
        }

        #endregion

        #region Ordenes Bancarias

        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        [RoleFilter(Roles: "StatisticsBankOrder")]
        //[UserTypeFilter("UserTypeP")]
        public IActionResult OrdenesBancarias()
        {
            return View("~/Views/Consultas/OrdenesBancarias/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> obtenerOrdenesBancariasTable(DateTime start, DateTime end, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var OrdenSurtimientoSeguimiento = new DataResult<IEnumerable<SupplyOrderDto>>();

                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("UserID", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", false));
                List<string> cleanedSearchList = new List<string>();
                if (search != null && search.Any())
                {
                    foreach (var item in search)
                    {
                        string removed = _generals.RemoveSpecialCharacters(item);
                        cleanedSearchList.Add(removed);
                    }
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                var org = _generals.User.Organisms.FirstOrDefault();
                string claveOrganismo = "";
                if (org != null) claveOrganismo = org.Clave;
                param.Add(new CustomHttpParameter("claveOrganismo", claveOrganismo));
                OrdenSurtimientoSeguimiento = await _utility.GetItem<DataResult<IEnumerable<SupplyOrderDto>>>("Consultas/GetListaOrdenesBancariasAsync", param);
                
                if (OrdenSurtimientoSeguimiento.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    OrdenSurtimientoSeguimiento.Pager = new Pager(OrdenSurtimientoSeguimiento?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={OrdenSurtimientoSeguimiento.Pager.TotalItems}");
                    OrdenSurtimientoSeguimiento.Pager = new Pager(OrdenSurtimientoSeguimiento.Pager.TotalItems, pageNum, pageSize);
                }

                return PartialView("~/Views/Consultas/OrdenesBancarias/GenerarTablaOrdenSurtimiento.cshtml", OrdenSurtimientoSeguimiento);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar las ordenes, por favor intente mas tarde" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelGetAllOrdenesBancarias(DateTime start, DateTime end, string search, int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var OrdenSurtimientoSeguimiento = new DataResult<IEnumerable<SupplyOrderDto>>();

                param.Add(new CustomHttpParameter("pageSize", 10000));
                param.Add(new CustomHttpParameter("pageNum", pageNum == 0 ? 1 : pageNum));
                param.Add(new CustomHttpParameter("UserID", _generals.User.UserID));
                var org = _generals.User.Organisms.FirstOrDefault();
                string claveOrganismo = "";
                if (org != null) claveOrganismo = org.Clave;
                param.Add(new CustomHttpParameter("claveOrganismo", claveOrganismo));

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("esDescarga", true));
                List<string> cleanedSearchList = new List<string>();
                var searchR = search.Split("__");
                foreach (var item in searchR)
                {
                    string removed = _generals.RemoveSpecialCharacters(item.Replace("_", ","));
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                OrdenSurtimientoSeguimiento = await _utility.GetItem<DataResult<IEnumerable<SupplyOrderDto>>>("Consultas/GetListaOrdenesBancariasAsync", param);

                int pageSize;
                if (OrdenSurtimientoSeguimiento.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, usando count de datos");
                    pageSize = OrdenSurtimientoSeguimiento?.Data?.Count() ?? 0;
                    OrdenSurtimientoSeguimiento.Pager = new Pager(pageSize, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={OrdenSurtimientoSeguimiento.Pager.TotalItems}");
                    pageSize = OrdenSurtimientoSeguimiento.Pager.TotalItems;
                    OrdenSurtimientoSeguimiento.Pager = new Pager(OrdenSurtimientoSeguimiento.Pager.TotalItems, pageNum, pageSize);
                }
                
                if (OrdenSurtimientoSeguimiento.Pager.TotalItems <= pageSize)
                {
                    OrdenSurtimientoSeguimiento.Pager.EndPage = 1;

                }

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Ordenes Bancarias");
                worksheet.Cell(1, 1).Value = "No. OS";
                worksheet.Cell(1, 2).Value = "No. Contrato";
                worksheet.Cell(1, 3).Value = "Organismo";
                worksheet.Cell(1, 4).Value = "Tipo";
                worksheet.Cell(1, 5).Value = "Acreedor";
                worksheet.Cell(1, 6).Value = "Total";
                worksheet.Cell(1, 7).Value = "Moneda";

                int index = 1;
                foreach (var item in OrdenSurtimientoSeguimiento.Data)
                {
                    worksheet.Cell(index + 1, 1).Value = "'" + item.SAPOrder;
                    worksheet.Cell(index + 1, 2).Value = "'" + item.Contract;
                    worksheet.Cell(index + 1, 3).Value = item.OrganismClave;
                    worksheet.Cell(index + 1, 4).Value = item.DocumentType == "M" ? "Medicamentos" : "Obras";
                    worksheet.Cell(index + 1, 5).Value = item.CreditorNumber;
                    worksheet.Cell(index + 1, 6).Value = item.Total;
                    worksheet.Cell(index + 1, 7).Value = item.Currency;
                    index++;
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrdenBancaria" + fileName + ".xlsx");
                }
            }
            catch (Exception )
            {
                return RedirectToAction("OrdenesBancarias");
            }
        }

        #endregion

        #region PaymentSchedule

        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        [RoleFilter(Roles: "StatisticsPaymentSchedule")]
        //[UserTypeFilter("UserTypeP")]
        public IActionResult ProgramaPago()
        {
            return View("~/Views/Consultas/PaymentSchedule/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> obtenerPaymentScheduleTable(DateTime start, DateTime end, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var PaymentSchedules = new DataResult<IEnumerable<ReportePaymentScheduleDto>>();

                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", false));
                List<string> cleanedSearchList = new List<string>();
                foreach (var item in search)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                PaymentSchedules = await _utility.GetItem<DataResult<IEnumerable<ReportePaymentScheduleDto>>>("Consultas/GetListaPaymentScheduleAsync", param);

                PaymentSchedules.Pager = new Pager(PaymentSchedules.Data.ToList().Count(), pageNum, pageSize);
                if (PaymentSchedules.Data.ToList().Count() <= pageSize)
                {
                    PaymentSchedules.Pager.EndPage = 1;

                }
                PaymentSchedules.Pager.CurrentPage = pageNum;
                PaymentSchedules.Data = PaymentSchedules.Data.Skip((pageNum - 1) * pageSize).Take(pageSize);
                return PartialView("~/Views/Consultas/PaymentSchedule/GenerarTablaPaymentSchedules.cshtml", PaymentSchedules);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar los rechazos, por favor intente mas tarde" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelGetAllPaymentSchedule(DateTime start, DateTime end, string search, int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var PaymentSchedules = new DataResult<IEnumerable<ReportePaymentScheduleDto>>();

                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", true));
                List<string> cleanedSearchList = new List<string>();
                var searchR = (search ?? "").Split("__");
                foreach (var item in searchR)
                {
                    string removed = _generals.RemoveSpecialCharacters(item.Replace("_", ","));
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                PaymentSchedules = await _utility.GetItem<DataResult<IEnumerable<ReportePaymentScheduleDto>>>("Consultas/GetListaPaymentScheduleAsync", param);

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Programa de Pago");
                worksheet.Cell(1, 1).Value = "Organismo";
                worksheet.Cell(1, 2).Value = "Consolidación ID";
                worksheet.Cell(1, 3).Value = "Fecha Programa";
                worksheet.Cell(1, 4).Value = "Fecha Pago";
                worksheet.Cell(1, 5).Value = "Func. Status";
                //worksheet.Cell(1, 1).Value = "Organismo";
                //worksheet.Cell(1, 2).Value = "Programa Pago";
                //worksheet.Cell(1, 3).Value = "Fecha de Pago";
                //worksheet.Cell(1, 4).Value = "Fecha Recepcion";
                //worksheet.Cell(1, 5).Value = "Fecha Programada";

                int index = 1;
                foreach (var item in PaymentSchedules.Data)
                {
                    worksheet.Cell(index + 1, 1).Value = item.Organismo;
                    worksheet.Cell(index + 1, 2).Value = item.ConsolidacionID;
                    worksheet.Cell(index + 1, 3).Value = item.FechaPrograma;
                    worksheet.Cell(index + 1, 4).Value = item.FechaPago;
                    worksheet.Cell(index + 1, 5).Value = item.FuncStatus ? "Firmado" : "No firmado";
                    //worksheet.Cell(index + 1, 1).Value = "'" + item.Organismo;
                    //worksheet.Cell(index + 1, 2).Value = item.ProgramaPago_Id;
                    //worksheet.Cell(index + 1, 3).Value = item.PaymentDate.ToString("dd/MM/yyyy hh:mm");
                    //worksheet.Cell(index + 1, 4).Value = item.ReceptionDate.ToString("dd/MM/yyyy hh:mm");
                    //worksheet.Cell(index + 1, 5).Value = item.ScheduleDate.ToString("dd/MM/yyyy hh:mm");
                    index++;
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProgramaPago" + fileName + ".xlsx");
                }
            }
            catch (Exception )
            {
                return RedirectToAction("ProgramaPago");
            }
        }

        #endregion

        #region PaymentList

        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        [RoleFilter(Roles: "StatisticsPaymentList")]
        //[UserTypeFilter("UserTypeP")]
        public IActionResult ListaPago()
        {
            return View("~/Views/Consultas/PaymentList/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> obtenerPaymentListTable(DateTime start, DateTime end, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var PaymentList = new DataResult<IEnumerable<ReportePaymentListDto>>();

                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", false));
                List<string> cleanedSearchList = new List<string>();
                foreach (var item in search)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                PaymentList = await _utility.GetItem<DataResult<IEnumerable<ReportePaymentListDto>>>("Consultas/GetListaPaymentListAsync", param);

                PaymentList.Pager = new Pager(PaymentList.Data.ToList().Count(), pageNum, pageSize);
                if (PaymentList.Data.ToList().Count() <= pageSize)
                {
                    PaymentList.Pager.EndPage = 1;

                }
                PaymentList.Pager.CurrentPage = pageNum;
                PaymentList.Data = PaymentList.Data.Skip((pageNum - 1) * pageSize).Take(pageSize);
                return PartialView("~/Views/Consultas/PaymentList/GenerarTablaPaymentList.cshtml", PaymentList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar los rechazos, por favor intente mas tarde" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelGetAllPaymentList(DateTime start, DateTime end, string search, int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var PaymentList = new DataResult<IEnumerable<ReportePaymentListDto>>();

                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", true));
                List<string> cleanedSearchList = new List<string>();
                var searchR = search.Split("__");
                foreach (var item in searchR)
                {
                    string removed = _generals.RemoveSpecialCharacters(item.Replace("_", ","));
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                PaymentList = await _utility.GetItem<DataResult<IEnumerable<ReportePaymentListDto>>>("Consultas/GetListaPaymentListAsync", param);

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Lista de Pago");
                worksheet.Cell(1, 1).Value = "Organismo";
                worksheet.Cell(1, 2).Value = "Lista de Pago";
                worksheet.Cell(1, 3).Value = "Fecha";
                worksheet.Cell(1, 4).Value = "Fecha Recepcion";

                int index = 1;
                foreach (var item in PaymentList.Data)
                {
                    worksheet.Cell(index + 1, 1).Value = "'" + item.Organismo;
                    worksheet.Cell(index + 1, 2).Value = item.ListaPago_Id;
                    worksheet.Cell(index + 1, 3).Value = item.Date.ToString("dd/MM/yyyy hh:mm");
                    worksheet.Cell(index + 1, 4).Value = item.ReceptionDate.ToString("dd/MM/yyyy hh:mm");
                    index++;
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListaPago" + fileName + ".xlsx");
                }
            }
            catch (Exception )
            {
                return RedirectToAction("ListaPago");
            }
        }

        #endregion

        #region OrdenSurtimiento
        [RoleFilter("StatisticsSupplyOrders")]
        public IActionResult OrdenSurtimiento(DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            ViewBag.FechaInicial = fechaInicial;
            ViewBag.FechaFinal = fechaFinal;
            List<string> srcList = new List<string>();
            List<string> cleanedSearchList = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                srcList = search.Split(";").ToList();
                foreach (var item in srcList)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
            }

            ViewBag.Search = cleanedSearchList.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            return View("OrdenSurtimiento/index");
        }
        // [HttpPost]
        // public async Task<IActionResult> OrdenSurtimientoConsultaTable(DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1, IEnumerable<string> search = null)
        // {
        //     try
        //     {
        //         fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
        //         fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
        //         var param = new List<CustomHttpParameter>();
        //         int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:OrdenSurtimiento").Value);
        //         param.Add(new CustomHttpParameter("pageSize", pageSize));
        //         param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
        //         param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
        //         param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
        //         param.Add(new CustomHttpParameter("pageNum", pageNum));
        //         List<string> cleanedSearchList = new List<string>();
        //         foreach (var item in search)
        //         {
        //             string removed = _generals.RemoveSpecialCharacters(item);
        //             cleanedSearchList.Add(removed);
        //         }
        //         string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
        //         param.Add(new CustomHttpParameter("search", _search));
        //         var ordenSurtimiento = await _utility.GetItem<DataResult<IEnumerable<SupplyOrderDto>>>("SupplyOrders/Consulta/GetOSAsync", param);
        //         if (ordenSurtimiento.Status != System.Net.HttpStatusCode.OK)
        //             return Json(new { success = false, message = "Ocurrió un error al consultar la información, por favor intente mas tarde." });
        //         
        //         if (ordenSurtimiento.Pager == null)
        //         {
        //             Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
        //             ordenSurtimiento.Pager = new Pager(ordenSurtimiento?.Data?.Count() ?? 0, pageNum, pageSize);
        //         }
        //         else
        //         {
        //             Log.Information($"✓ Pager recibido: TotalItems={ordenSurtimiento.Pager.TotalItems}");
        //             ordenSurtimiento.Pager = new Pager(ordenSurtimiento.Pager.TotalItems, pageNum, pageSize);
        //         }
        //         return PartialView("OrdenSurtimiento/_OrdenSurtimientoConsultaTable", ordenSurtimiento);
        //     }
        //     catch (Exception ex)
        //     {
        //         Log.Error(ex.Message);
        //         return Json(new { success = false, message = "Ocurrió un error al consultar la información, por favor intente mas tarde." });
        //     }
        // }
        
        [HttpPost]
        [Route("Consultas/OrdenSurtimiento/ExpedienteElectronico")]
        public async Task<IActionResult> ExpedienteElectronicoOrdenSurtimiento(string SAPOrder, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("SAPOrder", SAPOrder));
                var _result = await _utility.GetItem<DataResult<ExpedienteEViewModel>>("ExpedienteElectronico/GetExpediente", param);

                if (_result.Status != System.Net.HttpStatusCode.OK || _result.Data == null || _result.Data.SupplyOrder == null)
                    return RedirectToAction("OrdenSurtimiento", new { fechaInicial = fechaInicial, fechaFinal = fechaFinal, search = search });
                _result.Data.RedirectAction = "Consultas/OrdenSurtimiento";
                ViewBag.SAPOrder = SAPOrder;
                ViewBag.Search = search;
                ViewBag.FechaInicial = fechaInicial;
                ViewBag.FechaFinal = fechaFinal;
                return View("ExpedienteElectronico", _result.Data);
            }
            catch (Exception)
            {
                return RedirectToAction("OrdenSurtimiento");
            }
        }
        [HttpGet]
        public async Task<IActionResult> DownloadExcelOrdenSurtimiento(DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:OrdenSurtimiento").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                List<string> srcList = new List<string>();
                List<string> cleanedSearchList = new List<string>();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    srcList = search.Split(";").ToList();
                    foreach (var item in srcList)
                    {
                        string removed = _generals.RemoveSpecialCharacters(item);
                        cleanedSearchList.Add(removed);
                    }
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));
                param.Add(new CustomHttpParameter("esDescarga", true));
                var ordenSurtimiento = await _utility.GetItem<DataResult<IEnumerable<SupplyOrderDto>>>("SupplyOrders/Consulta/GetOSAsync", param);
                if (ordenSurtimiento.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al realizar la búsqueda, por favor intente mas tarde" });

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Órdenes de surtimiento");
                worksheet.Cell(1, 1).Value = "Organismo";
                worksheet.Cell(1, 2).Value = "Contrato";
                worksheet.Cell(1, 3).Value = "No. de Pedido";
                worksheet.Cell(1, 4).Value = "Moneda";
                worksheet.Cell(1, 5).Value = "Nombre de Acreedor";
                worksheet.Cell(1, 6).Value = "Total";
                worksheet.Cell(1, 7).Value = "Fecha de Recepción";
                worksheet.Cell(1, 8).Value = "Orden SIAF";
                worksheet.Cell(1, 9).Value = "Tipo";
                worksheet.Cell(1, 10).Value = "Fecha Firma del Funcionario";
                worksheet.Cell(1, 11).Value = "Fecha Firma del Proveedor";
                worksheet.Cell(1, 12).Value = "Estatus";
                worksheet.Cell(1, 13).Value = "Cancelado";
                worksheet.Cell(1, 14).Value = "Fecha Cancelación";
                worksheet.Cell(1, 15).Value = "Emitido por";
                worksheet.Cell(1, 16).Value = "Firmado por";
                worksheet.Cell(1, 17).Value = "No. de Serie";
                var os = ordenSurtimiento.Data.ToList();
                for (int index = 1; index <= os.Count; index++)
                {
                    worksheet.Cell(index + 1, 1).Value = os[index - 1].OrganismClave;
                    worksheet.Cell(index + 1, 2).Value = os[index - 1].Contract;
                    worksheet.Cell(index + 1, 3).Value = os[index - 1].SAPOrder;
                    worksheet.Cell(index + 1, 4).Value = os[index - 1].Currency;
                    worksheet.Cell(index + 1, 5).Value = os[index - 1].Creditor;
                    worksheet.Cell(index + 1, 6).Value = Convert.ToDouble(os[index - 1].Total).ToString("{0:#,##0.#0}");
                    worksheet.Cell(index + 1, 7).Value = os[index - 1].ReceptionDate;
                    worksheet.Cell(index + 1, 8).Value = os[index - 1].SIAFOrder;
                    worksheet.Cell(index + 1, 9).Value = os[index - 1].DocumentType;
                    worksheet.Cell(index + 1, 10).Value = os[index - 1].FunctionarySignDate;
                    worksheet.Cell(index + 1, 11).Value = os[index - 1].ProviderSignDate;
                    worksheet.Cell(index + 1, 12).Value = os[index - 1].FunctionarySignDate.HasValue && os[index - 1].ProviderSignDate.HasValue ? "Firmado" : (!os[index - 1].FunctionarySignDate.HasValue ? "Funcionario" : "Proveedor");
                    worksheet.Cell(index + 1, 13).Value = os[index - 1].IsCancel.HasValue && os[index - 1].IsCancel.Value ? "SI" : "NO";
                    worksheet.Cell(index + 1, 14).Value = os[index - 1].CancelDate;
                    worksheet.Cell(index + 1, 15).Value = os[index - 1].MadeBy;
                    worksheet.Cell(index + 1, 16).Value = os[index - 1].FunctionarySignerName;
                    worksheet.Cell(index + 1, 17).Value = os[index - 1].Signer;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Concat("OS", DateTime.Now.ToString("yyyyMMdd"), ".xlsx"));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al realizar la búsqueda, por favor intente mas tarde" });
            }
        }
        #endregion

        #region EstimacionObra
        [RoleFilter("StatisticsSOEstimations")]
        public IActionResult EstimacionObra(DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            ViewBag.FechaInicial = fechaInicial;
            ViewBag.FechaFinal = fechaFinal;
            List<string> srcList = new List<string>();
            List<string> cleanedSearchList = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                srcList = search.Split(";").ToList();
                foreach (var item in srcList)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
            }

            ViewBag.Search = cleanedSearchList.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            return View("EstimacionObra/Index");
        }
        [HttpPost]
        public async Task<IActionResult> EstimacionConsultaTable(DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:EstimacionObra").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("esDescarga", false));
                List<string> cleanedSearchList = new List<string>();
                if (search != null && search.Any())
                {
                    foreach (var item in search)
                    {
                        string removed = _generals.RemoveSpecialCharacters(item);
                        cleanedSearchList.Add(removed);
                    }
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                var result = await _utility.GetItem<DataResult<IEnumerable<SOEstimationDto>>>("SOEstimation/Consulta/GetESAsync", param);
                if (result.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al realizar la búsqueda, por favor intente mas tarde" });
                
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
                return PartialView("EstimacionObra/_EstimacionesTable", result);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al realizar la búsqueda, por favor intente mas tarde" });
            }
        }

        [Route("Consultas/EstimacionObra/ExpedienteElectronico")]
        public async Task<IActionResult> ExpedienteElectronicoEstimacionObra(string SAPOrder, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("SAPOrder", SAPOrder));
                var _result = await _utility.GetItem<DataResult<ExpedienteEViewModel>>("ExpedienteElectronico/GetExpediente", param);

                if (_result.Status != System.Net.HttpStatusCode.OK || _result.Data == null)
                    return RedirectToAction("Index", new { fechaInicial = fechaInicial, fechaFinal = fechaFinal, search = search });

                if (_result.Data.SOEstimation == null)
                    return RedirectToAction("Index", new { fechaInicial = fechaInicial, fechaFinal = fechaFinal, search = search });

                _result.Data.RedirectAction = "Consultas/EstimacionObra";
                ViewBag.SAPOrder = SAPOrder;
                ViewBag.Search = search;
                ViewBag.FechaInicial = fechaInicial;
                ViewBag.FechaFinal = fechaFinal;
                return View("ExpedienteElectronico", _result.Data);
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public async Task<IActionResult> DownloadExcelEstimacionObra(DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:OrdenSurtimiento").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                List<string> srcList = new List<string>();
                List<string> cleanedSearchList = new List<string>();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    srcList = search.Split(";").ToList();
                    foreach (var item in srcList)
                    {
                        string removed = _generals.RemoveSpecialCharacters(item);
                        cleanedSearchList.Add(removed);
                    }
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));
                param.Add(new CustomHttpParameter("esDescarga", true));
                var result = await _utility.GetItem<DataResult<IEnumerable<SOEstimationDto>>>("SOEstimation/Consulta/GetESAsync", param);
                if (result.Status != System.Net.HttpStatusCode.OK)
                    return RedirectToAction("Index", new { fechaInicial = fechaInicial, fechaFinal = fechaFinal, search = search });

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Estimaciones de obra");
                worksheet.Cell(1, 1).Value = "Orden Surtimiento";
                worksheet.Cell(1, 2).Value = "Contrato";
                worksheet.Cell(1, 3).Value = "Organismo";
                worksheet.Cell(1, 4).Value = "Tipo";
                worksheet.Cell(1, 5).Value = "Acreedor";
                worksheet.Cell(1, 6).Value = "Total";
                worksheet.Cell(1, 7).Value = "Moneda";
                var os = result.Data.ToList();
                for (int index = 1; index <= os.Count; index++)
                {
                    worksheet.Cell(index + 1, 1).Value = os[index - 1].SapOrder;
                    worksheet.Cell(index + 1, 2).Value = os[index - 1].Contract;
                    worksheet.Cell(index + 1, 3).Value = os[index - 1].OrganismClave;
                    worksheet.Cell(index + 1, 4).Value = os[index - 1].DocumentType;
                    worksheet.Cell(index + 1, 5).Value = os[index - 1].CreditorNumber;
                    worksheet.Cell(index + 1, 6).Value = os[index - 1].Total;
                    worksheet.Cell(index + 1, 7).Value = os[index - 1].Currency;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Concat("ES", DateTime.Now.ToString("yyyyMMdd"), ".xlsx"));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al realizar la búsqueda, por favor intente mas tarde" });
            }
        }
        #endregion

        #region Consulta Copade
        [RoleFilter(Roles: "StatisticsCopade")]
        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        public ActionResult Copade(DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            ViewBag.FechaInicial = fechaInicial;
            ViewBag.FechaFinal = fechaFinal;
            List<string> srcList = new List<string>();
            List<string> cleanedSearchList = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                srcList = search.Split(";").ToList();
                foreach (var item in srcList)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
            }

            ViewBag.Search = cleanedSearchList.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            return View("Copade/Index");
        }

        [HttpPost]
        public async Task<IActionResult> ConsultaCopadeTable(DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:ConsultaCopade").Value);
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("esDescarga", false));
                List<string> cleanedSearchList = new List<string>();
                foreach (var item in search)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));
                var ConsultaCopade = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>("ConsultaCopade/GetConsultaCopadeAsync", param);
                if (ConsultaCopade.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al realizar la búsqueda, por favor intente mas tarde" });
                
                if (ConsultaCopade.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    ConsultaCopade.Pager = new Pager(ConsultaCopade?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={ConsultaCopade.Pager.TotalItems}");
                    ConsultaCopade.Pager = new Pager(ConsultaCopade.Pager.TotalItems, pageNum, pageSize);
                }
                return PartialView("Copade/_ConsultaCopadeTable", ConsultaCopade);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al realizar la búsqueda, por favor intente mas tarde" });
            }
        }

        [Route("Consultas/Copade/ExpedienteElectronico")]
        public async Task<IActionResult> ExpedienteElectronicoCopade(Guid CopadeID, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null, string rutaRegreso = null, bool esSeguimiento = false)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("CopadeID", CopadeID));
                var _result = await _utility.GetItem<DataResult<ExpedienteEViewModel>>("ExpedienteElectronico/GetExpediente", param);

                if (_result.Status != System.Net.HttpStatusCode.OK || _result.Data == null)
                    return RedirectToAction("Index", new { fechaInicial = fechaInicial, fechaFinal = fechaFinal, search = search, });


                if (_result.Data.Copade == null)
                    return RedirectToAction("Index", new { fechaInicial = fechaInicial, fechaFinal = fechaFinal, search = search, });

                if (String.IsNullOrEmpty(rutaRegreso))
                {
                    _result.Data.RedirectAction = esSeguimiento == false ? "Consultas/Copade" : "Consultas/SeguimientoCopade";
                }
                else
                {
                    _result.Data.RedirectAction = rutaRegreso;
                }

                ViewBag.FechaInicial = fechaInicial;
                ViewBag.FechaFinal = fechaFinal;
                ViewBag.Search = search;
                return View("ExpedienteElectronico", _result.Data);
            }
            catch (Exception)
            {
                return RedirectToAction("Copade");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelCopade(DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:ConsultaCopade").Value);
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("pageNum", 1));
                param.Add(new CustomHttpParameter("esDescarga", true));

                var searchR = (search ?? "").Split("__");
                List<string> cleanedSearchList = new List<string>();
                foreach (var item in searchR)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));
                var ConsultaCopade = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>("ConsultaCopade/GetConsultaCopadeAsync", param);

                if (ConsultaCopade.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al realizar la descarga, por favor intente mas tarde" });

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Seguimiento COPADE");
                //worksheet.Cell(1, 1).Value = "Organismo";
                //worksheet.Cell(1, 2).Value = "Contrato";
                //worksheet.Cell(1, 3).Value = "Pedido";
                //worksheet.Cell(1, 4).Value = "Copade";
                //worksheet.Cell(1, 5).Value = "Nombre  Acreedor ";
                //worksheet.Cell(1, 6).Value = "No. Acreedor";
                //worksheet.Cell(1, 7).Value = "Ejercicio";
                //worksheet.Cell(1, 8).Value = "CxP";
                //worksheet.Cell(1, 9).Value = "Estatus";
                //worksheet.Cell(1, 10).Value = "Cancelado";
                //worksheet.Cell(1, 11).Value = "Factura Recibida";
                //worksheet.Cell(1, 12).Value = "Tipo de factura";
                //worksheet.Cell(1, 13).Value = "Fecha Factura";
                //worksheet.Cell(1, 14).Value = "fecha recepción de factura";
                //worksheet.Cell(1, 15).Value = "Serie";
                //worksheet.Cell(1, 16).Value = "Folio";
                //worksheet.Cell(1, 17).Value = "UUID";

                int index = 1;
                foreach (var item in ConsultaCopade.Data)
                {
                    //worksheet.Cell(index + 1, 1).Value = item.OrganismID;
                    //worksheet.Cell(index + 1, 2).Value = item.Contract;
                    //worksheet.Cell(index + 1, 3).Value = item.SapOrder;
                    //worksheet.Cell(index + 1, 4).Value = item.CopadeID;
                    //worksheet.Cell(index + 1, 5).Value = item.Creditor;
                    //worksheet.Cell(index + 1, 6).Value = item.CreditorNumber;
                    //worksheet.Cell(index + 1, 7).Value = item.Exercise;
                    //worksheet.Cell(index + 1, 8).Value = "???";
                    //worksheet.Cell(index + 1, 9).Value = (item.Functionary1SignDate.HasValue && item.Functionary2SignDate.HasValue ? "Firmado" : (item.Functionary1SignDate.HasValue ? "Funcionario 1" : (item.Functionary2SignDate.HasValue ? "Funcionario 2" : "")));
                    //worksheet.Cell(index + 1, 10).Value = (item.IsCancel.HasValue && item.IsCancel.Value ? "CANCELADO" : "NO");
                    //worksheet.Cell(index + 1, 11).Value = "???";
                    //worksheet.Cell(index + 1, 12).Value = "???";
                    //worksheet.Cell(index + 1, 13).Value = "???";
                    //worksheet.Cell(index + 1, 14).Value = "???";
                    //worksheet.Cell(index + 1, 15).Value = "???";
                    //worksheet.Cell(index + 1, 16).Value = "???";
                    //worksheet.Cell(index + 1, 17).Value = "???";
                    index++;
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "COPADE" + fileName + ".xlsx");
                }
            }
            catch (Exception )
            {
                return RedirectToAction("SeguimientoCopade");
            }
        }
        #endregion

        #region Seguimiento de copade
        [RoleFilter("QueryTracing")]
        public IActionResult SeguimientoCopade(DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            ViewBag.FechaInicial = fechaInicial;
            ViewBag.FechaFinal = fechaFinal;
            List<string> srcList = new List<string>();
            List<string> cleanedSearchList = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                srcList = search.Split(";").ToList();
                foreach (var item in srcList)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
            }

            ViewBag.Search = cleanedSearchList.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            return View("SeguimientoCopade/Index");
        }

        [HttpPost]
        public async Task<IActionResult> ConsultaSeguimientoCopadeTable(DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:ConsultaCopade").Value);
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("esDescarga", false));
                List<string> cleanedSearchList = new List<string>();
                foreach (var item in search)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));
                var ConsultaCopade = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>("ConsultaCopade/GetConsultaCopadeAsync", param);
                if (ConsultaCopade.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al realizar la búsqueda, por favor intente mas tarde" });
                
                if (ConsultaCopade.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    ConsultaCopade.Pager = new Pager(ConsultaCopade?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={ConsultaCopade.Pager.TotalItems}");
                    ConsultaCopade.Pager = new Pager(ConsultaCopade.Pager.TotalItems, pageNum, pageSize);
                }
                return PartialView("SeguimientoCopade/_ConsultaSeguimientoCopadeTable", ConsultaCopade);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al realizar la búsqueda, por favor intente mas tarde" });
            }
        }

        [Route("Consultas/SeguimientoCopade/Detalle")]
        public async Task<IActionResult> SeguimientoCopadeDetalle(Guid CopadeID, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("CopadeID", CopadeID));
                var detalle = await _utility.GetItem<DataResult<ExpedienteEViewModel>>("ExpedienteElectronico/GetExpediente", param);
                if (detalle.Status != System.Net.HttpStatusCode.OK || detalle.Data == null)
                    return RedirectToAction("Seguimiento", new { fechaInicial = fechaInicial, fechaFinal = fechaFinal, search = search, });
                if (detalle.Data.SupplyOrder != null)
                    detalle.Data.SupplyOrder.Seguimiento = SupplyOrderDates(detalle.Data.SupplyOrder);
                if (detalle.Data.SOEstimation != null)
                    detalle.Data.SOEstimation.Seguimiento = SOEstimationDates(detalle.Data.SOEstimation);
                if (detalle.Data.Copade != null)
                    detalle.Data.Copade.Seguimiento = CopadeDates(detalle.Data.Copade);

                detalle.Data.RedirectAction = "Consultas/SeguimientoCopade";
                ViewBag.FechaInicial = fechaInicial;
                ViewBag.FechaFinal = fechaFinal;
                ViewBag.Search = search;
                return View("SeguimientoCopade/Detalle", detalle.Data);
            }
            catch (Exception)
            {
                return RedirectToAction("SeguimientoCopade");
            }
        }

        private IEnumerable<DateHelperModel> SupplyOrderDates(SupplyOrderDto supplyOrder)
        {
            var allDates = new List<DateHelperModel> {
                 new DateHelperModel {Order = 1, Title="Recepción", Description = "Recepción de órden de surtimiento", Date = supplyOrder.ReceptionDate, Info = supplyOrder.SAPOrder },
                 new DateHelperModel {Order = 2, Title="Email", Description = "Envío de email del funcionario", Date = supplyOrder.FunctionaryEmailSendDate, Info = supplyOrder.FunctionarySignerName },
                 new DateHelperModel {Order = 3, Title="Firma", Description = "Firma de órden de surtimiento por el funcionario", Date = supplyOrder.FunctionarySignDate, Info = string.Concat(supplyOrder.FunctionarySignerName, " con número de ficha ", supplyOrder.FunctionaryFicha) },
                 new DateHelperModel {Order = 4, Title="Notificación", Description = "Notificación a Pemex de firma del funcionario", Date = supplyOrder.FunctionaryNotifyPemexDate, Info = supplyOrder.FunctionarySignerName },
                 new DateHelperModel {Order = 5, Title="Email", Description = "Envío de email del proveedor", Date = supplyOrder.ProviderEmailSendDate, Info = supplyOrder.ProviderSignerName },
                 new DateHelperModel {Order = 6, Title="Firma", Description = "Fecha de firma del proveedor", Date = supplyOrder.ProviderSignDate, Info = string.Concat(supplyOrder.ProviderSignerName, " con número de acreedor ", supplyOrder.CreditorNumber) },
                 new DateHelperModel {Order = 7, Title="Notificación", Description = "Notificación a Pemex de firma del proveedor", Date = supplyOrder.ProviderNotifyPemexDate, Info = supplyOrder.ProviderSignerName },
                 new DateHelperModel {Order = 8, Title="Cancelación", Description = "Cancelación de orden de surtimiento", Date = supplyOrder.CancelDate, Info = supplyOrder.FunctionaryCancelName },
                new DateHelperModel { Order = 9, Title = "Liberación VP", Description = "Fecha de Liberación VP", Date = supplyOrder.LiberacionVPDate }
            };
            return allDates.Where(x => !string.IsNullOrWhiteSpace(x.Date.ToString())).OrderBy(x => x.Order).ToList();
        }
        
        private IEnumerable<DateHelperModel> SOEstimationDates(SOEstimationDto soEstimation)
        {
            var allDates = new List<DateHelperModel> {
                 new DateHelperModel {Order = 1, Title="Recepción", Description = "Recepción de estimación de obra", Date = soEstimation.ReceptionDate, Info = soEstimation.SapOrder },
                 new DateHelperModel {Order = 2, Title="Email", Description = "Envío de email al proveedor", Date = soEstimation.ProviderEmailSendDate, Info = soEstimation.ProviderSignerName },

                 new DateHelperModel {Order = 3, Title="Firma", Description = "Fecha de firma del proveedor", Date = soEstimation.ProviderSignDate, Info = string.Concat(soEstimation.ProviderSignerName, " con número de acreedor ", soEstimation.CreditorNumber) },

                 new DateHelperModel {Order = 4, Title="Notificación", Description = "Notificación a Pemex de firma del proveedor", Date = soEstimation.ProviderNotifyPemexDate, Info = soEstimation.ProviderSignerName },

                 new DateHelperModel {Order = 5, Title="Email", Description = "Envío de email del funcionario", Date = soEstimation.FunctionaryEmailSendDate, Info = soEstimation.FunctionarySignerName },

                 new DateHelperModel {Order = 6, Title="Firma", Description = "Firma de estimación de obra por el funcionario", Date = soEstimation.FunctionarySignDate, Info = string.Concat(soEstimation.FunctionarySignerName, " con número de ficha ", soEstimation.Signer) },

                 new DateHelperModel {Order = 7, Title="Notificación", Description = "Notificación a Pemex de firma del funcionario", Date = soEstimation.FunctionaryNotifyPemexDate, Info = soEstimation.FunctionarySignerName },

                 new DateHelperModel {Order = 8, Title="Cancelación", Description = "Cancelación de estimación de obra", Date = soEstimation.CancelDate, Info = soEstimation.FunctionaryCancelName }
            };
            return allDates.Where(x => !string.IsNullOrWhiteSpace(x.Date.ToString())).OrderBy(x => x.Order).ToList();
        }
        
        private IEnumerable<DateHelperModel> CopadeDates(CopadeDto copade)
        {
            var allDates = new List<DateHelperModel> {
                new DateHelperModel {Order = 1, Title="Recepción", Description = "Recepción copade", Date = copade.ReceptionDate, Info = copade.Reception },
                new DateHelperModel {Order = 2, Title="Email", Description = "Envío de email al funcionario", Date = copade.Functionary1EmailSendDate, Info = string.Concat(copade.Functionary1SignerName, " con correo ", copade.Functionary1Email) },
                new DateHelperModel {Order = 3, Title="Firma", Description = "Firma de copade por el funcionario", Date = copade.Functionary1SignDate, Info = string.Concat(copade.Functionary1SignerName, " con número de ficha ", copade.Functionary1Ficha) },
                 new DateHelperModel {Order = 4, Title="Email", Description = "Envío de email al funcionario", Date = copade.Functionary2EmailSendDate, Info = string.Concat(copade.Functionary2SignerName, " con correo ", copade.Functionary2Email) },
                 new DateHelperModel {Order = 5, Title="Firma", Description = "Firma de copade por el funcionario", Date = copade.Functionary2SignDate, Info = string.Concat(copade.Functionary2SignerName, " con número de ficha ", copade.Functionary2Ficha) },
                new DateHelperModel {Order = 6, Title="Email", Description = "Envío de prefactura al proveedor", Date = copade.ProviderEmailSendDate},
                new DateHelperModel {Order = 7, Title="Cancelación", Description = "Cancelación de copade", Date = copade.CancelDate, Info = string.Concat(" se cancela el copade por el usuario ", copade.FunctionaryCancelName, " con número de ficha ", copade.CancelBy) }
            };
            return allDates.Where(x => !string.IsNullOrWhiteSpace(x.Date.ToString())).OrderBy(x => x.Order).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelSeguimientoCopade(DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:ConsultaCopade").Value);
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("pageNum", 1));
                param.Add(new CustomHttpParameter("esDescarga", true));

                var searchR = (search ?? "").Split("__");
                List<string> cleanedSearchList = new List<string>();
                foreach (var item in searchR)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                var ConsultaCopade = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>("ConsultaCopade/GetConsultaCopadeAsync", param);

                if (ConsultaCopade.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al realizar la descarga, por favor intente mas tarde" });

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Seguimiento COPADE");
                worksheet.Cell(1, 1).Value = "Organismo";
                worksheet.Cell(1, 2).Value = "Contrato";
                worksheet.Cell(1, 3).Value = "Pedido";
                worksheet.Cell(1, 4).Value = "Copade";
                worksheet.Cell(1, 5).Value = "Nombre  Acreedor ";
                worksheet.Cell(1, 6).Value = "No. Acreedor";
                worksheet.Cell(1, 7).Value = "Ejercicio";
                worksheet.Cell(1, 8).Value = "CxP";
                worksheet.Cell(1, 9).Value = "Estatus";
                worksheet.Cell(1, 10).Value = "Cancelado";
                worksheet.Cell(1, 11).Value = "Factura Recibida";
                worksheet.Cell(1, 12).Value = "Tipo de factura";
                worksheet.Cell(1, 13).Value = "Fecha Factura";
                worksheet.Cell(1, 14).Value = "fecha recepción de factura";
                worksheet.Cell(1, 15).Value = "Serie";
                worksheet.Cell(1, 16).Value = "Folio";
                worksheet.Cell(1, 17).Value = "UUID";

                int index = 1;
                foreach (var item in ConsultaCopade.Data)
                {
                    //Revisión con Alejandro Rico: Seguimiento COPADE, COPADE y COPADE Bancario
                    worksheet.Cell(index + 1, 1).Value = item.clave;
                    worksheet.Cell(index + 1, 2).Value = item.Contract;
                    worksheet.Cell(index + 1, 3).Value = item.SapOrder;
                    worksheet.Cell(index + 1, 4).Value = item.Reception;
                    worksheet.Cell(index + 1, 5).Value = item.Creditor;
                    worksheet.Cell(index + 1, 6).Value = item.CreditorNumber;
                    worksheet.Cell(index + 1, 7).Value = item.Exercise;
                    worksheet.Cell(index + 1, 8).Value = "???";
                    worksheet.Cell(index + 1, 9).Value = (item.Functionary1SignDate.HasValue && item.Functionary2SignDate.HasValue ? "Firmado" : (item.Functionary1SignDate.HasValue ? "Funcionario 1" : (item.Functionary2SignDate.HasValue ? "Funcionario 2" : "")));
                    worksheet.Cell(index + 1, 10).Value = (item.IsCancel.HasValue && item.IsCancel.Value ? "CANCELADO" : "NO");
                    worksheet.Cell(index + 1, 11).Value = "???";
                    worksheet.Cell(index + 1, 12).Value = "???";
                    worksheet.Cell(index + 1, 13).Value = "???";
                    worksheet.Cell(index + 1, 14).Value = "???";
                    worksheet.Cell(index + 1, 15).Value = "???";
                    worksheet.Cell(index + 1, 16).Value = "???";
                    worksheet.Cell(index + 1, 17).Value = "???";
                    index++;
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SeguimientoCOPADE" + fileName + ".xlsx");
                }
            }
            catch (Exception )
            {
                return RedirectToAction("SeguimientoCopade");
            }
        }
        #endregion

        #region EstadoFacturas

        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        [RoleFilter(Roles: "StatisticsReceptionInvoice")]
        //[UserTypeFilter("UserTypeP")]
        public IActionResult EstadoFacturas(bool isRedirect = false, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            ViewBag.isRedirectEstadoFacturas = isRedirect.ToString();

            return View("~/Views/Consultas/EstadoFacturas/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> obtenerEstadoFacturasTable(DateTime start, DateTime end, int pageNum = 1, IEnumerable<string> search = null)
        {
            ViewBag.isRedirectEstadoFacturas = false;
            if (start == DateTime.MinValue)
            {
                TempData["CFFechaInicial"] = null;
            }
            else
            {
                TempData["CFFechaInicial"] = start;
            }
            if (end == DateTime.MinValue)
            {
                TempData["CFFechaFinal"] = null;
            }
            else
            {
                TempData["CFFechaFinal"] = end;
            }
            if (search == null)
            {
                TempData["CFSearch"] = search;
            }
            else
            {
                TempData["CFSearch"] = search.ToList<string>();
            }

            try
            {
                var param = new List<CustomHttpParameter>();
                var EstadoFacturas = new DataResult<IEnumerable<EstadoFacturaDto>>();

                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("source", 1));      // 0 - factura, 1 - analitico

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", false));
                List<string> cleanedSearchList = new List<string>();
                foreach (var item in search)
                {
                    string removed = _generals.RemoveSpecialCharacters(item);
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                EstadoFacturas = await _utility.GetItem<DataResult<IEnumerable<EstadoFacturaDto>>>("Consultas/GetEstadoFacturasAsync", param);

                EstadoFacturas.Pager = new Pager(EstadoFacturas.Data.ToList().Count(), pageNum, pageSize);
                if (EstadoFacturas.Data.ToList().Count() <= pageSize)
                {
                    EstadoFacturas.Pager.EndPage = 1;

                }
                EstadoFacturas.Pager.CurrentPage = pageNum;
                EstadoFacturas.Data = EstadoFacturas.Data.Skip((pageNum - 1) * pageSize).Take(pageSize);
                return PartialView("~/Views/Consultas/EstadoFacturas/GenerarTablaEstadoFacturas.cshtml", EstadoFacturas);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar el listado de facturas, por favor intente mas tarde" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcelGetAllEstadoFacturas(DateTime start, DateTime end, int pageNum = 1, string search = "")
        {
            try
            {
                ViewBag.isRedirectEstadoFacturas = false;

                var param = new List<CustomHttpParameter>();
                var EstadoFacturas = new DataResult<IEnumerable<EstadoFacturaDto>>();

                param.Add(new CustomHttpParameter("pageSize", 10000));
                param.Add(new CustomHttpParameter("pageNum", 1));
                param.Add(new CustomHttpParameter("source", 1));      // 0 - factura, 1 - analitico

                if (start == DateTime.MinValue) start = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (end == DateTime.MinValue) end = DateTime.Now;
                string _start = start.Year.ToString() + start.Month.ToString().PadLeft(2, '0') + start.Day.ToString().PadLeft(2, '0');
                string _end = end.Year.ToString() + end.Month.ToString().PadLeft(2, '0') + end.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                param.Add(new CustomHttpParameter("esDescarga", true));
                List<string> cleanedSearchList = new List<string>();
                var searchR = search.Split("__");
                foreach (var item in searchR)
                {
                    string removed = _generals.RemoveSpecialCharacters(item.Replace("_", ","));
                    cleanedSearchList.Add(removed);
                }
                string _search = Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
                param.Add(new CustomHttpParameter("search", _search));

                EstadoFacturas = await _utility.GetItem<DataResult<IEnumerable<EstadoFacturaDto>>>("Consultas/GetEstadoFacturasAsync", param);

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Status Facturas");
                worksheet.Cell(1, 1).Value = "Documento";
                worksheet.Cell(1, 2).Value = "Origen";          //Copade o Analitico
                worksheet.Cell(1, 3).Value = "Tipo";            //Electronica o Documental
                worksheet.Cell(1, 4).Value = "Fecha Recepcion";
                worksheet.Cell(1, 5).Value = "Fecha factura";
                worksheet.Cell(1, 6).Value = "Documento SAP";
                worksheet.Cell(1, 7).Value = "Comentario";      //por si tiene error 210 o 220
                worksheet.Cell(1, 8).Value = "Folio";           //serie - folio
                worksheet.Cell(1, 9).Value = "UUID";
                worksheet.Cell(1, 10).Value = "Total";
                worksheet.Cell(1, 11).Value = "Correo Env.";

                int index = 1;
                foreach (var item in EstadoFacturas.Data)
                {
                    worksheet.Cell(index + 1, 1).Value = "'" + item.Documento;
                    worksheet.Cell(index + 1, 2).Value = item.Origen;
                    worksheet.Cell(index + 1, 3).Value = item.Tipo;
                    worksheet.Cell(index + 1, 4).Value = item.FechaRecepcion.ToString("dd/MM/yyyy hh:mm");
                    worksheet.Cell(index + 1, 5).Value = item.FechaFactura.ToString("dd/MM/yyyy hh:mm");
                    worksheet.Cell(index + 1, 6).Value = item.SapOrder;
                    worksheet.Cell(index + 1, 7).Value = item.Comentario;
                    worksheet.Cell(index + 1, 8).Value = item.Folio;
                    worksheet.Cell(index + 1, 9).Value = item.UUID.ToString();
                    worksheet.Cell(index + 1, 10).Value = item.Total;
                    worksheet.Cell(index + 1, 11).Value = item.CorreoEnviado;
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EstadoFacturas" + fileName + ".xlsx");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("EstadoFacturas");
            }
        }

        #endregion

        #region Consulta AnaliticoPago
        [RoleFilter(Roles: "StatisticsAnalyticalPayment")]
        //[UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        public ActionResult AnaliticoPago(DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null)
        {
            ViewBag.FechaInicial = fechaInicial;
            ViewBag.FechaFinal = fechaFinal;
            ViewBag.Search = search;
            return View("AnaliticoPago/AnaliticoPago");
        }
        public async Task<IActionResult> GetAPFactura(DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1, string search = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:AnaliticoPago").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("ficha", _generals.User.Token));
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("search", search));

                var ap = await _utility.GetItem<DataResult<IEnumerable<AnaliticoPagoDto>>>("AnaliticoPago/Consulta/GetAPAsync", param);
                if (ap.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al realizar la búsqueda, por favor intente mas tarde" });
                
                if (ap.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    ap.Pager = new Pager(ap?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={ap.Pager.TotalItems}");
                    ap.Pager = new Pager(ap.Pager.TotalItems, pageNum, pageSize);
                }
                return PartialView("AnaliticoPago/_APFactura", ap);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al cargar el analitico de pago, por favor intente mas tarde" });
            }
        }

        [Route("Consultas/AnaliticoPago/ExpedienteElectronico")]
        public async Task<IActionResult> ExpedienteElectronicoAP(Guid AnaliticoPagoID, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null, string rutaRegreso = null, bool esSeguimiento = false)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("AnaliticoPagoID", AnaliticoPagoID));
                var _result = await _utility.GetItem<DataResult<ExpedienteEViewModel>>("ExpedienteElectronico/GetExpediente", param);

                if (_result.Status != System.Net.HttpStatusCode.OK || _result.Data == null)
                    return RedirectToAction("AnaliticoPago", new { search = search, });


                if (_result.Data.AnaliticoPago == null)
                    return RedirectToAction("AnaliticoPago", new { search = search, });

                if (String.IsNullOrEmpty(rutaRegreso))
                {
                    _result.Data.RedirectAction = esSeguimiento == false ? "Consultas/AnaliticoPago" : "Consultas/SeguimientoCopade";
                }
                else
                {
                    _result.Data.RedirectAction = rutaRegreso;
                }

                ViewBag.FechaInicial = fechaInicial;
                ViewBag.FechaFinal = fechaFinal;
                ViewBag.Search = search;
                return View("ExpedienteElectronico", _result.Data);
            }
            catch (Exception)
            {
                return RedirectToAction("AnaliticoPago");
            }
        }
        #endregion
    }
}
