using BERecepcion.Api.Filters;
using BERecepcion.Api.HtmlHelpers;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.Instrucciones.Dto;
using iText.Html2pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using QRCoder;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BERecepcion.Api.Controllers.FirmaDocumentos
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class DocumentoPDFController : Controller
    {
        private readonly IDocumentosRepository _documentosRepository;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _configuration;
        public DocumentoPDFController(IDocumentosRepository documentosRepository, IHostEnvironment env, IConfiguration configuration)
        {
            _documentosRepository = documentosRepository;
            _env = env;
            _configuration = configuration;
        }
        [HttpGet("GetDocumentoAsync")]
        [ProducesResponseType(typeof(DataResult<ArchivoPDFDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentoAsync(string SAPOrder, string Organismo)
        {
            try
            {
                return Ok(await _documentosRepository.GetDocumentoAsync(SAPOrder, Organismo));
            }
            catch (Exception ex)
            {
                Log.Error("DocumentoPDF: GetDocumentoAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetDocumentoProgramaPagoAsync")]
        [ProducesResponseType(typeof(DataResult<ArchivoPDFDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentoProgramaPagoAsync(string PPId, string Organismo)
        {
            try
            {
                return Ok(await _documentosRepository.GetDocumentoProgramaPagoAsync(PPId, Organismo));
            }
            catch (Exception ex)
            {
                Log.Error("DocumentoPDF: GetDocumentoProgramaPagoAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetDocumentoESignAsync")]
        [ProducesResponseType(typeof(DataResult<ArchivoPDFDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentoESignAsync(string paqueteId, string documentoId)
        {
            try
            {
                var docPDF = await _documentosRepository.GetDocumentoESignAsync(paqueteId, documentoId);
                ArchivoPDFDto archivoPDF = new ArchivoPDFDto { ARCHIVO = Convert.ToBase64String(docPDF.RawBytes) };
                DataResult<ArchivoPDFDto> resPdf = new DataResult<ArchivoPDFDto> { Data = archivoPDF };

                return Ok(resPdf);
            }
            catch (Exception ex)
            {
                Log.Error("DocumentoPDF: GetDocumentoAsync {error}", ex.ToString());
                throw;
            }
        }
        [HttpGet("GetDocumentoPruebaAsync")]
        [ProducesResponseType(typeof(DataResult<ArchivoPDFDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentoPruebaAsync()
        {
            try
            {
                try
                {
                    DataResult<ArchivoPDFDto> resultItem = new DataResult<ArchivoPDFDto>()
                    {
                        Status = System.Net.HttpStatusCode.OK,
                        Data = new ArchivoPDFDto()
                    };
                    string path = _env.ContentRootPath + "\\Files\\prueba.pdf";
                    Byte[] bytes = System.IO.File.ReadAllBytes(path);
                    resultItem.Data.ARCHIVO = Convert.ToBase64String(bytes);
                    await Task.CompletedTask;
                    return Ok(resultItem);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Log.Error("DocumentoPDF: GetDocumentoPruebaAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetDocumentoAnaliticoPagoAsync")]
        [ProducesResponseType(typeof(DataResult<ArchivoPDFDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentoAnaliticoPagoAsync(string CveTransportista, string NumCliente, string Ejercicio, string Analitico)
        {
            //< itransport > 00307 </ itransport >     - CveTransportista
            //< icte > 0001300228 </ icte >            - NumCliente
            //< ianio > 2020 </ ianio >                - Ejercicio
            //< ilistado_emb > 90059 </ ilistado_emb > - Analitico
            //< token >$Fletes_8l % 21ue </ token >

            try
            {
                return Ok(await _documentosRepository.GetDocumentoAPAsync(CveTransportista, NumCliente, Ejercicio, Analitico));
            }
            catch (Exception ex)
            {
                Log.Error("DocumentoPDF: GetDocumentoAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetDocumentoFacturaAsync")]
        [ProducesResponseType(typeof(DataResult<List<ComprobanteDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentoFacturaAsync(string InvoiceId, string tipo)
        {
            List<string> result = new List<string>() { };
            try
            {
                BERecepcion.Core.Facturas.Dto.ComprobanteDto documentoXML = null;
                if (tipo == "I")
                {
                    documentoXML = await _documentosRepository.GetDocumentoFacturaAsync(InvoiceId);
                }
                else if (tipo == "N")
                {
                    documentoXML = await _documentosRepository.GetDocumentoNotaCreditoAsync(InvoiceId);
                }

                XmlSerializer serializer = null;
                if(documentoXML.CFDIVersion== "3.3")
                {
                    serializer = new XmlSerializer(typeof(ComprobanteBE));
                }
                else if (documentoXML.CFDIVersion == "4.0" || String.IsNullOrEmpty(documentoXML.CFDIVersion))
                {
                    serializer = new XmlSerializer(typeof(ComprobanteBE40));
                }
                
                using (TextReader reader = new StringReader(documentoXML.OriginalXml))
                {
                    bool deserialized = false;
                    ComprobanteBE rBE = null;
                    ComprobanteBE40 rBE40 = null;
                    if (documentoXML.CFDIVersion == "3.3")
                    {
                        rBE = (ComprobanteBE)serializer.Deserialize(reader);
                        deserialized = true;
                    }
                    else if (documentoXML.CFDIVersion == "4.0" || String.IsNullOrEmpty(documentoXML.CFDIVersion))
                    {
                        rBE40 = (ComprobanteBE40)serializer.Deserialize(reader);
                        deserialized = true;
                    }

                    if (deserialized)
                    {
                        string viewIndex = "~/Views/FacturaPDF/Factura.cshtml";
                        string renderized = "";

                        if (documentoXML.CFDIVersion == "3.3")
                        {
                            renderized = adjustEncoding(this.RenderViewAsync(viewIndex, rBE, false).Result);
                        }
                        else if (documentoXML.CFDIVersion == "4.0" || String.IsNullOrEmpty(documentoXML.CFDIVersion))
                        {
                            viewIndex = "~/Views/FacturaPDF/Factura40.cshtml";
                            renderized = adjustEncoding(this.RenderViewAsync(viewIndex, rBE40, false).Result);
                        }

                        byte[] applicationPDFData = Encoding.ASCII.GetBytes(renderized);
                        string archivo = Guid.NewGuid().ToString();
                        string filePath = "Files/" + archivo + ".html";
                        FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
                        fileStream.Write(applicationPDFData, 0, applicationPDFData.Length);
                        fileStream.Close();

                        HtmlConverter.ConvertToPdf(
                            new FileInfo(@"files/" + archivo + ".html"),
                            new FileInfo(@"files/" + archivo + ".pdf")
                        );

                        byte[] bytes = null;
                        using (FileStream fsSource = new FileStream("Files/" + archivo + ".pdf", FileMode.Open, FileAccess.Read))
                        {

                            bytes = new byte[fsSource.Length];
                            int numBytesToRead = (int)fsSource.Length;
                            int numBytesRead = 0;
                            while (numBytesToRead > 0)
                            {
                                int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);
                                if (n == 0)
                                    break;

                                numBytesRead += n;
                                numBytesToRead -= n;
                            }
                            numBytesToRead = bytes.Length;
                        }

                        result.Add(Convert.ToBase64String(bytes));

                        DataResult<FacturaPdfDto> dataResult = new DataResult<FacturaPdfDto>();
                        dataResult.Data = new FacturaPdfDto();
                        dataResult.Data.result = result;
                        dataResult.Status = System.Net.HttpStatusCode.OK;
                        await Task.CompletedTask;

                        try
                        {
                            if (System.IO.File.Exists("Files/" + archivo + ".pdf"))
                                System.IO.File.Delete("Files/" + archivo + ".pdf");
                            if (System.IO.File.Exists("Files/" + archivo + ".html"))
                                System.IO.File.Delete("Files/" + archivo + ".html");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                        }

                        return Ok(dataResult);
                    }
                }
                await Task.CompletedTask;
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("FacturaPDFController: FacturaPDF {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetDocumentoComplementoPagoAsync")]
        [ProducesResponseType(typeof(DataResult<List<ComprobanteDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentoComplementoPagoAsync(string PagoUUID)
        {
            List<string> result = new List<string>() { };
            try
            {
                var dato = _documentosRepository.GetDocumentoComplementoPagoAsync(PagoUUID);
                string prueba = dato.Result.OriginalXml;


                XmlSerializer serializer = new XmlSerializer(typeof(ComprobanteBE));

                using (TextReader reader = new StringReader(prueba))
                {
                    ComprobanteBE r = (ComprobanteBE)serializer.Deserialize(reader);
                    if (r != null)
                    {
                        ComprobanteBE factura = new ComprobanteBE();
                        factura = r;
                        string resultados = factura.Complemento.TimbreFiscalDigital.SelloCFD.Substring(factura.Complemento.TimbreFiscalDigital.SelloCFD.Length - 8);

                        string token = _configuration["GenerarQR:urlSATQR"];
                        string t = String.Format("{0:#,#########0.#####0}", Convert.ToDouble(1.2));
                        if (t.Length != 16)
                        {
                            var ceros = 16 - t.Length;
                            for (int i = 0; ceros >= i; i++)
                            {
                                string prue = "0";
                                t = prue + t;
                            }
                        }
                        string txtvalor = token + "&id=" + factura.Complemento.TimbreFiscalDigital.UUID + "&re=" + factura.Emisor.Rfc + "&rr=" + factura.Receptor.Rfc + "&tt=" + t + "&fe=" + resultados;
                        var QR = GenerateQRCode(txtvalor);

                        string viewIndex = "~/Views/FacturaPDF/Comprobante.cshtml";

                        string renderized = adjustEncoding(this.RenderViewAsync(viewIndex, factura, false).Result);

                        byte[] applicationPDFData = Encoding.ASCII.GetBytes(renderized);
                        string archivo = Guid.NewGuid().ToString();
                        string filePath = "Files/" + archivo + ".html";
                        FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
                        fileStream.Write(applicationPDFData, 0, applicationPDFData.Length);
                        fileStream.Close();

                        HtmlConverter.ConvertToPdf(
                            new FileInfo(@"files/" + archivo + ".html"),
                            new FileInfo(@"files/" + archivo + ".pdf")
                        );

                        byte[] bytes = null;
                        using (FileStream fsSource = new FileStream("Files/" + archivo + ".pdf", FileMode.Open, FileAccess.Read))
                        {

                            bytes = new byte[fsSource.Length];
                            int numBytesToRead = (int)fsSource.Length;
                            int numBytesRead = 0;
                            while (numBytesToRead > 0)
                            {
                                int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);
                                if (n == 0)
                                    break;

                                numBytesRead += n;
                                numBytesToRead -= n;
                            }
                            numBytesToRead = bytes.Length;
                        }

                        result.Add(Convert.ToBase64String(bytes));

                        try
                        {
                            if (System.IO.File.Exists("Files/" + archivo + ".pdf"))
                                System.IO.File.Delete("Files/" + archivo + ".pdf");
                            if (System.IO.File.Exists("Files/" + archivo + ".html"))
                                System.IO.File.Delete("Files/" + archivo + ".html");
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.Message;
                        }

                        DataResult<FacturaPdfDto> dataResult = new DataResult<FacturaPdfDto>();
                        dataResult.Data = new FacturaPdfDto();
                        dataResult.Data.result = result;
                        dataResult.Status = System.Net.HttpStatusCode.OK;

                        return Ok(dataResult);
                    }
                }
                await Task.CompletedTask;
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("DocumentoPDF: GetDocumentoComplementoPagoAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
        private string adjustEncoding(string c)
        {
            return c.Replace("á", "&aacute;").Replace("é", "&eacute;").Replace("í", "&iacute;").Replace("ó", "&oacute;").Replace("ú", "&uacute;").Replace("ñ", "&ntilde;").Replace("Á", "&Aacute;").Replace("É", "&Eacute;").Replace("Í", "&Iacute;").Replace("Ó", "&Oacute;").Replace("Ú", "&Uacute;").Replace("Ñ", "&Ntilde;");
        }

        private IActionResult GenerateQRCode(string txtvalor)
        {
            QRCodeGenerator qRCodeGenerador = new QRCodeGenerator();
            QRCodeData qRCodeData = qRCodeGenerador.CreateQrCode(txtvalor, QRCodeGenerator.ECCLevel.Q);
            
            QRCode qrCode = new QRCode(qRCodeData);
            Bitmap bitmap = qrCode.GetGraphic(15);
            var bitmapBytes = ConvertBitmapToBytes(bitmap);
            string result1 = Convert.ToBase64String(bitmapBytes);
            ViewBag.QR = result1;
            return null;
            //return File(bitmapBytes, "image/jpeg");

        }
        private byte[] ConvertBitmapToBytes(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        [HttpGet("GetDocumentoListaPagoAsync")]
        [ProducesResponseType(typeof(DataResult<List<ReportePaymentList>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentoListaPagoAsync(string ListaPago_id)
        {
            List<string> result = new List<string>() { };
            try
            {
                var ListaPago = await _documentosRepository.GetDocumentoListaPagoAsync(ListaPago_id);
                ////consulta imagen en BD
                ///
                string clave = "PEP";
                var Imagen = await _documentosRepository.GetImagenAsync(clave);
                ViewBag.Imagen = Imagen;

                List<P_DetallePep> ls = new List<P_DetallePep>();
                ls = JsonConvert.DeserializeObject<List<P_DetallePep>>(ListaPago.DetallePep);
                string d = ListaPago.ReceptionDate.ToString("d MMMM", CultureInfo.CreateSpecificCulture("es-MX"));
                ViewBag.fecha = d + " del " + ListaPago.ReceptionDate.ToString("yyyy");
                ListaPago.vDetallePep = ls;
                string viewIndex = "~/Views/ListaPago/ListaPago.cshtml";

                string renderized = adjustEncoding(this.RenderViewAsync(viewIndex, ListaPago, false).Result);

                byte[] applicationPDFData = Encoding.ASCII.GetBytes(renderized);
                string archivo = Guid.NewGuid().ToString();
                string filePath = "Files/" + archivo + ".html";
                FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
                fileStream.Write(applicationPDFData, 0, applicationPDFData.Length);
                fileStream.Close();

                HtmlConverter.ConvertToPdf(
                    new FileInfo(@"files/" + archivo + ".html"),
                    new FileInfo(@"files/" + archivo + ".pdf")
                );

                byte[] bytes = null;
                using (FileStream fsSource = new FileStream("Files/" + archivo + ".pdf", FileMode.Open, FileAccess.Read))
                {

                    bytes = new byte[fsSource.Length];
                    int numBytesToRead = (int)fsSource.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);
                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                    numBytesToRead = bytes.Length;
                }

                result.Add(Convert.ToBase64String(bytes));

                try
                {
                    if (System.IO.File.Exists("Files/" + archivo + ".pdf"))
                        System.IO.File.Delete("Files/" + archivo + ".pdf");
                    if (System.IO.File.Exists("Files/" + archivo + ".html"))
                        System.IO.File.Delete("Files/" + archivo + ".html");
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }

                DataResult<FacturaPdfDto> dataResult = new DataResult<FacturaPdfDto>();
                dataResult.Data = new FacturaPdfDto();
                dataResult.Data.result = result;
                dataResult.Status = System.Net.HttpStatusCode.OK;
                await Task.CompletedTask;
                return Ok(dataResult);
               
            }
            catch (Exception ex)
            {
                Log.Error("FacturaPDFController: GetDocumentoListaPagoAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
    }
}