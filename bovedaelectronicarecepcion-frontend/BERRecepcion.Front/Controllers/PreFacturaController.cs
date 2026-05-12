using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BERRecepcion.Front.Controllers
{
    public class PreFacturaController : Controller
    {

        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        protected readonly IConfiguration _configuration;

        public PreFacturaController( IRestUtility utility, IGenerals generals, IConfiguration configuration)
        {
            _utility = utility;
            _generals = generals;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConsultaPreFactura(DateTime start, DateTime end, int pageNum = 1, IEnumerable<string> search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:PreFactura").Value);
                if (pageSize == 0) pageSize = 10;

                param.Add(new CustomHttpParameter("creditorNumber", _generals.User.CreditorNumber));
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

                var centgest = await _utility.GetItem<DataResult<IEnumerable<PreFacturaDto>>>("PreFacturaProveedor", param);

                centgest.Pager = new Pager(centgest.Pager.TotalItems == 0 ? 0 : centgest.Pager.TotalItems, pageNum, pageSize);

                return PartialView("_PreFacturaTable", centgest);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al cargar las Pre Facturas, por favor intente mas tarde" });
            }

        }

        [HttpPost]
        public async Task<IActionResult> BuscarPreFactura(IEnumerable<string> search = null, int pageNum = 1, DateTime? fechaInicial = null, DateTime? fechaFinal = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:PreFactura").Value);
                if (pageSize == 0) pageSize = 10;

                param.Add(new CustomHttpParameter("creditorNumber", _generals.User.CreditorNumber));
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
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

                var centgest = await _utility.GetItem<DataResult<IEnumerable<PreFacturaDto>>>("PreFacturaProveedor", param);

                centgest.Pager = new Pager(centgest.Pager.TotalItems == 0 ? 0 : centgest.Pager.TotalItems, pageNum, pageSize);

                return PartialView("_PreFacturaTable", centgest);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al cargar las Pre Facturas, por favor intente mas tarde" });
            }

        }

        [HttpGet]
        public async Task<IActionResult> DownloadXml(Guid CopadeID, bool tipo)
        {

            var param = new List<CustomHttpParameter>();
            param.Add(new CustomHttpParameter("CopadeID", CopadeID));
            var perfil = await _utility.GetItem<DataResult<PreFacturaDto>>("PreFacturaProveedor/GetPreFacturaXmlAsync", param);

            var xmlTipo = (tipo == true ? perfil.Data.PreFacturaXML : perfil.Data.NotaCreditoXML);
            
            return File(Encoding.UTF8.GetBytes(xmlTipo), "application/xml", CopadeID + ".xml");

        }

        [HttpGet]
        public async Task<IActionResult> DownloadZip(Guid CopadeID, bool tipo)
        {

            var param = new List<CustomHttpParameter>();
            param.Add(new CustomHttpParameter("CopadeID", CopadeID));
            var perfil = await _utility.GetItem<DataResult<PreFacturaDto>>("PreFacturaProveedor/GetPreFacturaXmlAsync", param);

            using (MemoryStream output = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(output, ZipArchiveMode.Create, true))
                {
                    // Esto va agregando los archivos al zip
                    var entry1 = zip.CreateEntry(CopadeID + "-PreFactura.xml");
                    using (var entryStream = entry1.Open())
                    using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
                    {
                        streamWriter.Write(perfil.Data.PreFacturaXML);
                    }
                    
                    if(perfil.Data.NotaCreditoXML != null)
                    {
                        var entry2 = zip.CreateEntry(CopadeID + "-NotaCredito.xml");
                        using (var entryStream = entry2.Open())
                        using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
                        {
                            streamWriter.Write(perfil.Data.NotaCreditoXML);
                        }
                    }
                }
              
                var nombreDelZip = "PreFacturas.zip";
                return File(output.ToArray(), "application/zip", nombreDelZip);
            }
        }

        public FileResult ShowZip(string filename, string PreFacturaXML, string NotaCreditoXML)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(output, ZipArchiveMode.Create, true))
                {
                    int cont = 1, contN = 1;
                 
                    foreach (var item in PreFacturaXML.Split(','))
                    {
                        if (item != "")
                        {
                            var entry = zip.CreateEntry(cont + "-PreFactura.xml");
                            using (var entryStream = entry.Open())
                            using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
                            {
                                streamWriter.Write(item);
                            }
                            cont++;
                        }
                    }

                    if (NotaCreditoXML != null)
                    {
                        foreach (var item in NotaCreditoXML.Split(','))
                        {
                            if (item != "")
                            {
                                var entry = zip.CreateEntry(contN + "-NotaCredito.xml");
                                using (var entryStream = entry.Open())
                                using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
                                {
                                    streamWriter.Write(item);
                                }
                                contN++;
                            }
                        }
                    }
                }

                var nombreDelZip = "PreFacturas.zip";
                return File(output.ToArray(), "application/zip", nombreDelZip);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadXmlZip(string dto)
        {
            try
            {
                var r = dto.Split("_");
                List<PreXmlMasDto> CopadesID = new List<PreXmlMasDto>() { };
                for (int i = 0; i < r.Length; i++)
                {
                    CopadesID.Add(new PreXmlMasDto() { CopadeID = Guid.Parse(r[i]), Status = true });
                }

                using (MemoryStream output = new MemoryStream())
                {
                    using (ZipArchive zip = new ZipArchive(output, ZipArchiveMode.Create, true))
                    {
                        var dataResult = new DataResult<IEnumerable<PreXmlMasDto>>
                        {
                            Data = CopadesID,
                            User = _generals.User,
                        };

                        var result = await _utility.Post(dataResult, "PreFacturaProveedor/GetPreFacturaXmlMasAsync");

                        int cont = 1, contN = 1;

                        foreach (var item in result.Data)
                        {
                            if (!String.IsNullOrEmpty(item.PreFacturaXML))
                            {
                                var entry = zip.CreateEntry(item.Copade + "-PreFactura.xml");
                                using (var entryStream = entry.Open())
                                using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
                                {
                                    streamWriter.Write(item.PreFacturaXML);
                                }
                                cont++;
                            }
                            if (!String.IsNullOrEmpty(item.NotaCreditoXML))
                            {
                                var entry = zip.CreateEntry(item.Copade + "-NotaCredito(" + contN + ").xml");
                                using (var entryStream = entry.Open())
                                using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
                                {
                                    streamWriter.Write(item.NotaCreditoXML);
                                }
                                contN++;
                            }
                        }
                    }

                    var nombreDelZip = "PreFacturas.zip";
                    var content = output.ToArray();
                    return File(content, "application/zip", nombreDelZip);
                }

            }
            catch (Exception ext)
            {
                return Json(new { Success = false, Message = ext.Message + "Ocurrió un error al Activar/Desactivar las interfases, por favor intente más tarde." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcel(DateTime fechaInicial, DateTime fechaFinal, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:PreFactura").Value);
                if (pageSize == 0) pageSize = 10;

                param.Add(new CustomHttpParameter("creditorNumber", _generals.User.CreditorNumber));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", 1));
                if (fechaInicial == DateTime.MinValue) fechaInicial = new DateTime(Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(0, 4)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(5, 2)), Convert.ToInt32(_configuration.GetSection("infoAplicativo:FechaInicial").Value.Substring(8, 2)));
                if (fechaFinal == DateTime.MinValue) fechaFinal = DateTime.Now;
                string _start = fechaInicial.Year.ToString() + fechaInicial.Month.ToString().PadLeft(2, '0') + fechaInicial.Day.ToString().PadLeft(2, '0');
                string _end = fechaFinal.Year.ToString() + fechaFinal.Month.ToString().PadLeft(2, '0') + fechaFinal.Day.ToString().PadLeft(2, '0');
                param.Add(new CustomHttpParameter("start", _start));
                param.Add(new CustomHttpParameter("end", _end));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
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

                var prefacturas = await _utility.GetItem<DataResult<IEnumerable<PreFacturaDto>>>("PreFacturaProveedor", param);

                if (prefacturas.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al realizar la descarga, por favor intente mas tarde" });

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("PreFacturas");
                worksheet.Cell(1, 1).Value = "Organismo";
                worksheet.Cell(1, 2).Value = "Pedido";
                worksheet.Cell(1, 3).Value = " Copade";
                worksheet.Cell(1, 4).Value = "Nombre  Acreedor ";
                worksheet.Cell(1, 5).Value = "No. Acreedor";
                worksheet.Cell(1, 6).Value = "Ejercicio";
                worksheet.Cell(1, 7).Value = "Centro Gestor";
                worksheet.Cell(1, 8).Value = "Fecha de recepción de Copade";
                worksheet.Cell(1, 9).Value = "Fecha firma funcioario 1";
                worksheet.Cell(1, 10).Value = "Fecha firma funcioario 2";
                worksheet.Cell(1, 11).Value = "Fecha de envio correo al proveedor";

                int index = 1;
                foreach (var item in prefacturas.Data)
                {
                    worksheet.Cell(index + 1, 1).Value = item.OrganismID.ToString();
                    worksheet.Cell(index + 1, 2).Value = item.SapOrder;
                    worksheet.Cell(index + 1, 3).Value = item.CopadeID.ToString();
                    worksheet.Cell(index + 1, 4).Value = item.Creditor;
                    worksheet.Cell(index + 1, 5).Value = item.CreditorNumber;
                    worksheet.Cell(index + 1, 6).Value = item.Exercise;
                    worksheet.Cell(index + 1, 7).Value = item.Center;
                    worksheet.Cell(index + 1, 8).Value = item.ReceptionDate;
                    worksheet.Cell(index + 1, 9).Value = item.functionary1signdate;
                    worksheet.Cell(index + 1, 10).Value = item.functionary2signdate;
                    worksheet.Cell(index + 1, 11).Value = item.ProviderEmailSendDate;
                    index++;
                }

                string fileName = DateTime.Now.ToString("ddMMyyyy");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PreFacturas" + fileName + ".xlsx");
                }
            }
            catch (Exception )
            {
                return RedirectToAction("Index");
            }
        }
    }
}
