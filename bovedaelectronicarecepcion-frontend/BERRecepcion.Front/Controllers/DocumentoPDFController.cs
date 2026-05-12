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
using Microsoft.Extensions.Hosting;
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
    public class DocumentoPDFController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        private readonly IHostEnvironment _env;

        public DocumentoPDFController(IConfiguration configuration, IRestUtility utility, IGenerals generals, IHostEnvironment env)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
            _env = env;
        }
        public async Task<IActionResult> GetDocumentoPDF(string SAPOrder, string Organismo, Guid? DocumentoBEId = null)
        {
            try
            {
                DataResult<ArchivoPDFDto> pdf = new DataResult<ArchivoPDFDto>();
                var param = new List<CustomHttpParameter>();

                if (!string.IsNullOrWhiteSpace(DocumentoBEId.ToString()))
                {
                    // recuperar el documento firmado
                    // aqui recuperamos el PDF  - segunda firma
                    // ESign/RecuperaPDFFirmadoEFirmaAsync
                    // recuperar el DocumentoFirmadoID (CorrelationID)                   
                    param.Add(new CustomHttpParameter("DocumentoBEId", DocumentoBEId.ToString()));
                    var documentoFirmado = await _utility.GetItem<DataResult<DocumentoFirmadoDto>>("DocumentoFirmado/GetDocumentoFirmadoAsync", param);                    
                    var resultRecuperado = await _utility.GetItem<DataResult<ArchivoPDFDto>>($"ESign/RecuperaPDFFirmadoEFirmaAsync/{documentoFirmado.Data.DocumentoFirmadoID}");
                    if (resultRecuperado.Status == System.Net.HttpStatusCode.OK)
                        return Json(new { success = true, file = resultRecuperado.Data.ARCHIVO });
                }
                else
                {
                    // firma 1                    
                    param.Add(new CustomHttpParameter("SAPOrder", SAPOrder));
                    param.Add(new CustomHttpParameter("Organismo", Organismo));
                    pdf = await _utility.GetItem<DataResult<ArchivoPDFDto>>("DocumentoPDF/GetDocumentoAsync", param);

                    if (pdf.Status == System.Net.HttpStatusCode.OK)
                        return Json(new { success = true, file = pdf.Data.ARCHIVO });
                }

                return Json(new { success = false, message = pdf.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener el archivo, por favor intente más tarde." });
            }
        }

        public async Task<IActionResult> GetDocumentoAnaliticoPagoPDF(string CveTransportista, string NumCliente, string Ejercicio, string Analitico)
        {
            try
            {
                //< itransport > 00307 </ itransport >     - CveTransportista
                //< icte > 0001300228 </ icte >            - NumCliente
                //< ianio > 2020 </ ianio >                - Ejercicio
                //< ilistado_emb > 90059 </ ilistado_emb > - Analitico
                //< token >$Fletes_8l % 21ue </ token >
                //CveTransportista = "00307";
                //NumCliente = "0001300228";
                //Ejercicio = "2020";
                //Analitico = "90059";
                // mandar parametros
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("CveTransportista", CveTransportista));
                param.Add(new CustomHttpParameter("NumCliente", NumCliente));
                param.Add(new CustomHttpParameter("Ejercicio", Ejercicio));
                param.Add(new CustomHttpParameter("Analitico", Analitico));

                var pdf = await _utility.GetItem<DataResult<ArchivoPDFDto>>("DocumentoPDF/GetDocumentoAnaliticoPagoAsync", param);

                if (pdf.Status == System.Net.HttpStatusCode.OK)
                    return Json(new { success = true, file = pdf.Data.ARCHIVO });
                return Json(new { success = false, message = pdf.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener el archivo, por favor intente más tarde." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetDocumentoByDocumentoBEId(Guid DocumentoBEId)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("DocumentoBEId", DocumentoBEId));
                var documentoFirmado = await _utility.GetItem<DataResult<DocumentoFirmadoDto>>("DocumentoFirmado/GetDocumentoFirmadoAsync", param);
                if (documentoFirmado == null || documentoFirmado.Data == null)
                    return Json(new { success = false, message = "Ocurrió un error al obtener el archivo, por favor intente más tarde." });
                var resultRecuperado = await _utility.GetItem<DataResult<ArchivoPDFDto>>($"ESign/RecuperaPDFFirmadoAsync/{documentoFirmado.Data.paqueteId}/{documentoFirmado.Data.documentoId}");
                if (resultRecuperado.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = resultRecuperado.Message });
                return Json(new { success = true, file = resultRecuperado.Data.ARCHIVO });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener el archivo, por favor intente más tarde." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetDocumentoAPByDocumentoBEId(Guid DocumentoBEId)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("DocumentoBEId", DocumentoBEId));
                var documentoFirmado = await _utility.GetItem<DataResult<DocumentoFirmadoDto>>("DocumentoFirmado/GetDocumentoAPFirmadoAsync", param);
                if (documentoFirmado == null)
                    return Json(new { success = false, message = "Ocurrió un error al obtener el archivo, por favor intente más tarde." });
                if (documentoFirmado.Data == null)
                    return Json(new { success = false, message = "Ocurrió un error al obtener el archivo, por favor intente más tarde." });
                var resultRecuperado = await _utility.GetItem<DataResult<ArchivoPDFDto>>($"ESign/RecuperaPDFAPFirmadoAsync/{documentoFirmado.Data.paqueteId}/{documentoFirmado.Data.documentoId}");
                if (resultRecuperado.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = resultRecuperado.Message });
                return Json(new { success = true, file = resultRecuperado.Data.ARCHIVO });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener el archivo, por favor intente más tarde." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> FacturaPDF(string InvoiceId)
        {

            var param = new List<CustomHttpParameter>();
            param.Add(new CustomHttpParameter("InvoiceId", InvoiceId));
            param.Add(new CustomHttpParameter("tipo", "I"));
            try
            {
                var result = await _utility.GetItem<DataResult<FacturaPdfDto>>("DocumentoPDF/GetDocumentoFacturaAsync", param);

                if (result.Status == System.Net.HttpStatusCode.OK)
                    return Json(new { success = true, file = result.Data.result.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return Json(new { success = false, message = "Ocurrio un error al recuperar la factura" });
        }
        [HttpGet]
        public async Task<IActionResult> NotaCreditoPDF(string NotaCreditoId)
        {
            FacturaPdfDto dtoPreview = new FacturaPdfDto();

            var param = new List<CustomHttpParameter>();
            param.Add(new CustomHttpParameter("InvoiceId", NotaCreditoId));
            param.Add(new CustomHttpParameter("tipo", "N"));

            dtoPreview.UsuarioModificador = _generals.User.Token;
            try
            {
                var result = await _utility.GetItem<DataResult<FacturaPdfDto>>("DocumentoPDF/GetDocumentoFacturaAsync", param);

                if (result.Status == System.Net.HttpStatusCode.OK)
                    return Json(new { success = true, file = result.Data.result.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return Json(new { success = false, message = "Ocurrio un error al recuperar la nota de credito" });
        }
        [HttpGet]
        public async Task<string> ComprobantePDF(string PagosID)
        {
            FacturaPdfDto dtoPreview = new FacturaPdfDto();

            var param = new List<CustomHttpParameter>();
            param.Add(new CustomHttpParameter("PagoUUID", PagosID));

            try
            {
                var result = await _utility.GetItem<DataResult<FacturaPdfDto>>("DocumentoPDF/GetDocumentoComplementoPagoAsync", param);

                if (result.Status == System.Net.HttpStatusCode.OK)
                    return result.Data.result.FirstOrDefault();

                return "";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return "";
        }

        [HttpGet]
        public async Task<string> ConsultaPagoList(string ListaPago_Id)
        {
            FacturaPdfDto dtoPreview = new FacturaPdfDto();

            var param = new List<CustomHttpParameter>();
            param.Add(new CustomHttpParameter("ListaPago_Id", ListaPago_Id));

            try
            {
                var result = await _utility.GetItem<DataResult<FacturaPdfDto>>("DocumentoPDF/GetDocumentoListaPagoAsync", param);

                if (result.Status == System.Net.HttpStatusCode.OK)
                    return result.Data.result.FirstOrDefault();

                return "";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return "";
        }

        [HttpPost]
        public async Task<string> ConsultaProgramacionPagoList([FromBody] ReportePaymentSchedule[] dto)
        {
            FacturaPdfDto dtoPreview = new FacturaPdfDto();

            var param = new List<CustomHttpParameter>();
            param.Add(new CustomHttpParameter("PPId", dto.ElementAt(0).ProgramaPago_Id));
            param.Add(new CustomHttpParameter("Organismo", dto.ElementAt(0).Organismo));

            try
            {
                var result = await _utility.GetItem<DataResult<ArchivoPDFDto>>("DocumentoPDF/GetDocumentoProgramaPagoAsync", param);

                if (result.Status == System.Net.HttpStatusCode.OK)
                    return result.Data.ARCHIVO;

                return "";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return "";
        }

        public async Task<IActionResult> GetDocumentoPP(string PPId, string Organismo)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("PPId", PPId));
                param.Add(new CustomHttpParameter("Organismo", Organismo));
                var pdf = await _utility.GetItem<DataResult<ArchivoPDFDto>>("DocumentoPDF/GetDocumentoProgramaPagoAsync", param);
                if (pdf.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = pdf.Message });
                return Json(new { success = true, file = pdf.Data.ARCHIVO });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener el archivo, por favor intente más tarde." });
            }
        }
        public async Task<IActionResult> GetDocumentoLP(string ListaPago_id)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("ListaPago_id", ListaPago_id));
                var pdf = await _utility.GetItem<DataResult<FacturaPdfDto>>("DocumentoPDF/GetDocumentoListaPagoAsync", param);
                if (pdf.Status != System.Net.HttpStatusCode.OK || pdf.Data.result.FirstOrDefault() == null)
                    return Json(new { success = false, message = pdf.Message });
                return Json(new { success = true, file = pdf.Data.result.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener el archivo, por favor intente más tarde." });
            }
        }
    }
}
