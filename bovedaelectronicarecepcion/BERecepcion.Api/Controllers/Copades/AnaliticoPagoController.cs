using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Correos.Dto;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BERecepcion.Api.Extensions;
using System.Net;

namespace BERecepcion.Api.Controllers.Copades
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class AnaliticoPagoController : ControllerBase
    {
        private readonly IAnaliticoPagoRepository _analiticoPagoRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly IConfiguration _configuration;
        private readonly IDocumentosRepository _documentosRepository;
        private readonly IHostEnvironment _env;
        private readonly IESignRepository _eSignRepository;
        private readonly IUsuariosRepository _usuariosRepository;
        private readonly ICorreoRepository _correoRepository;
        public AnaliticoPagoController(IAnaliticoPagoRepository analiticoPagoRepository, IBitacoraRepository bitacoraRepository, IConfiguration configuration, IDocumentosRepository documentosRepository, IHostEnvironment env, IESignRepository eSignRepository, IUsuariosRepository usuariosRepository, ICorreoRepository correoRepository)
        {
            _analiticoPagoRepository = analiticoPagoRepository;
            _bitacoraRepository = bitacoraRepository;
            _configuration = configuration;
            _documentosRepository = documentosRepository;
            _env = env;
            _eSignRepository = eSignRepository;
            _usuariosRepository = usuariosRepository;
            _correoRepository = correoRepository;
        }

        [HttpGet("ConsultaAPGetAllAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<AnaliticoPagoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultaAPGetAllAsync(int pageSize, string ficha, int pageNum = 1, string search = null)
        {
            var result = await _analiticoPagoRepository.ConsultaAPGetAllAsync(pageSize, ficha, pageNum, search);
            return result.ToActionResult();
        }

        [HttpGet("EmailPendientes")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<AnaliticoPagoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CorreoPendienteAsync(Guid AnaliticoPagoID)
        {
            // AnaliticoPAgo
            return Ok(await _analiticoPagoRepository.GetEmailAP(AnaliticoPagoID));
        }

        [HttpPost("EmailPreFactura")]
        [ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CorreoPreFacturaAsync([FromBody] string idAnalitico)
        {
            // AnaliticoPAgo
            // revisar si se manda el PDF de Analitico
            return Ok();
        }

        [HttpGet("GetAPByIdAnaliticoAsync/{idAnalitico}")]
        [ProducesResponseType(typeof(DataResult<AnaliticoPagoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAPByIdAnaliticoAsync(string idAnalitico)
        {
            var result = await _analiticoPagoRepository.GetAPByIdAnaliticoAsync(idAnalitico);
            return result.ToActionResult();
        }

        [HttpPost("FirmaUnoAsync")]
        [ProducesResponseType(typeof(DataResult<ExternosDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FirmaUnoAsync([FromBody] DataResult<ExternosDto> model)
        {
            DataResult<ExternosDto> resultItem = new DataResult<ExternosDto>
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso FirmaUnoAsync"
            };

            Paquete paquete = new Paquete(_configuration);
                DocumentoRepositorio documentoRepositorio = new DocumentoRepositorio();
                documentoRepositorio.TipoDocId = int.Parse(_configuration["eSign:TipoDocumento:tipoDocId:COPADE"]);   // cambiar al de AP
                PaqueteFirmante paqueteFirmante = new PaqueteFirmante(_configuration);
                //Armado y asignacion de paquete
                paquete.Titulo = model.Data.analiticoPago.Analitico;
                paquete.Descripcion = model.Data.analiticoPago.Analitico;
                paquete.UsuarioAlta = model.Data.usuario.Id;
                model.Data.paquete = paquete;

                //Armado y asignacion de documentoRepositorio
                documentoRepositorio.Titulo = model.Data.analiticoPago.Analitico;
                documentoRepositorio.Descripcion = model.Data.analiticoPago.Analitico;
                documentoRepositorio.UsuarioAlta = model.Data.usuario.Id;
                model.Data.documentoRepositorio = documentoRepositorio;

                //Armado y asignacion de paqueteFirmante
                paqueteFirmante.UsuarioId = model.Data.usuario.Id;
                model.Data.paqueteFirmante = paqueteFirmante;


                IFormFile documentoPDF;
                // Se suba el PDF recuperado
                var archivoPDF = await _documentosRepository.GetDocumentoAPAsync(model.Data.analiticoPago.CveTransportista, model.Data.analiticoPago.NumCliente, model.Data.analiticoPago.Ejercicio, model.Data.analiticoPago.Analitico);

                byte[] bytes = Convert.FromBase64String(archivoPDF.Data.ARCHIVO);
                string basePath = _env.ContentRootPath + "/Files/";
                bool basePathExists = Directory.Exists(basePath);
                if (!basePathExists) Directory.CreateDirectory(basePath);
                string archivo = basePath + model.Data.analiticoPago.Analitico + ".pdf";

                // Se guarda el PDF en el server (temporal)
                System.IO.File.WriteAllBytes(archivo, bytes);

                using (var stream = System.IO.File.OpenRead(archivo))
                {
                    documentoPDF = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "application/pdf"
                    };

                    var eSign = await _eSignRepository.PostDocumentoAsync(model.Data, documentoPDF);
                    if (eSign.Status != System.Net.HttpStatusCode.OK)
                    {
                        Log.Error("Externos PostDocumentoAsync eSign: {error}", eSign.Message);
                        return eSign.ToActionResult();
                    }
                    eSign.Data.documentoFirmaBE = model.Data.documentoFirmaBE;
                    eSign.Data.analiticoPago = model.Data.analiticoPago;
                    resultItem = eSign;
                    //System.IO.File.Delete(archivo);
                }
                return Ok(resultItem);
        }

        [HttpPost("CompletaFirmaAsync")]
        [ProducesResponseType(typeof(DataResult<ExternosDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompletaFirmaAsync([FromBody] DataResult<ExternosDto> externos)
        {
            DataResult<ExternosDto> resultItem = new DataResult<ExternosDto>
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso"
            };

            externos.Data.documentoFirma.UsuarioAlta = int.Parse(_configuration["eSign:Usuarios:usuarioAltaId"]);

                var analiticoPago = await _analiticoPagoRepository.GetAPByIdAnaliticoAsync(externos.Data.analiticoPago.IdAnalitico);
                var creditorUsers = await _usuariosRepository.GetUsuariosByCreditorNumber(externos.Data.analiticoPago.NumAcreedor);

                List<string> archivos = new List<string>() { };
                string filePdf = "";

                try
                {
                    var resultRecuperado = _documentosRepository.GetDocumentoAPAsync(analiticoPago.Data.CveTransportista, analiticoPago.Data.NumCliente, analiticoPago.Data.Ejercicio, analiticoPago.Data.Analitico);
                    byte[] pdfDataPre = Convert.FromBase64String(resultRecuperado.Result.Data.ARCHIVO);
                    filePdf = "Files/" + externos.Data.analiticoPago.IdAnalitico.ToString() + ".pdf";
                    FileStream fileStreamPdf = new FileStream(filePdf, FileMode.Create, FileAccess.ReadWrite);
                    fileStreamPdf.Write(pdfDataPre, 0, pdfDataPre.Length);
                    fileStreamPdf.Close();
                    archivos.Add(filePdf);

                    string xmlPre = analiticoPago.Data.PreFacturaXML;
                    if (!string.IsNullOrEmpty(xmlPre))
                    {
                        byte[] xmlDataPre = Encoding.Default.GetBytes(xmlPre);

                        string fileName = "Prefactura " + analiticoPago.Data.IdAnalitico.ToString();
                        string fileXml = "Files/" + fileName + ".xml";
                        FileStream fileStreamXml = new FileStream(fileXml, FileMode.Create, FileAccess.ReadWrite);
                        fileStreamXml.Write(xmlDataPre, 0, xmlDataPre.Length);
                        fileStreamXml.Close();
                        archivos.Add(fileXml);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }

                DtoPdfFilesEmail dtoPdfFilesEmail = new DtoPdfFilesEmail()
                {
                    analitico = analiticoPago.Data,
                    archivos = archivos,
                    users = creditorUsers.Data,
                    correoMensaje = new CorreoDto()
                };

                dtoPdfFilesEmail.correoMensaje.Subject = setCompletaFirmaSubject(analiticoPago.Data.Contrato, "", analiticoPago.Data.IdAnalitico, analiticoPago.Data.Moneda);

                // El analitico de pago solo tiene 1 firma
                var eSignFirma = await _eSignRepository.PostFirmaAsync(externos.Data, "1");
                if (eSignFirma.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("Externos PostDocumentoAsync eSignFirma: {error}", eSignFirma.Message);
                    return eSignFirma.ToActionResult();
                }

                // Guardamos al firmante y la fecha de firma
                await _analiticoPagoRepository.APFirmaAsync(externos.Data.analiticoPago.AnaliticoPagoID, externos.User.Token);
                // escribimos en bitacora
                await _bitacoraRepository.InsertaBitacoraAsync(externos.Data.documentoFirmaBE.Bitacora);

                // Hacer el envio del correo al proveedor, puede ser 1 o muchos usuarios
                var mail = await _correoRepository.EnvioCorreoAPArchivosAsync(dtoPdfFilesEmail);
                eSignFirma.Status = mail ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Conflict;
                // registramos evento de correo
                await _analiticoPagoRepository.APFirmaCorreoAsync(externos.Data.analiticoPago.AnaliticoPagoID, creditorUsers.Data);

                //borrado de archivos
                foreach (var fil in archivos)
                {
                    if (fil.ToLower().Contains(".xml") || fil.ToLower().Contains(".pdf"))
                    {
                        try
                        {
                            if (System.IO.File.Exists(fil)) System.IO.File.Delete(fil);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                        }
                    }
                }

                return Ok(resultItem);
        }

        [HttpGet("GetAPByIdAnaliticoClaveAsync/{idAnalitico}/{clave}")]
        [ProducesResponseType(typeof(IEnumerable<AnaliticoPagoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAPByIdAnaliticoClaveAsync(string idAnalitico, string clave)
        {
            var result = await _analiticoPagoRepository.GetAPByIdAnaliticoClaveAsync(idAnalitico, clave);
            return result.ToActionResult();
        }

        #region Consulta
        [HttpGet("Consulta/GetAPAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<AnaliticoPagoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAPAsync(int pageSize, string ficha, DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1, string search = null, bool esDescarga = false)
        {
            var result = await _analiticoPagoRepository.GetAPAsync(pageSize, ficha, pageNum, fechaInicial, fechaFinal, search, esDescarga);
            return result.ToActionResult();
        }
        #endregion
        private string setCompletaFirmaSubject(string Contract, string SupplyOrder, string IdAnalitico, string Currency)
        {
            string r, contract, supplyOrder, idAnalitico, currency;
            r = contract = supplyOrder = idAnalitico = currency = "";

            if (!string.IsNullOrEmpty(Contract)) contract = Contract;
            if (!string.IsNullOrEmpty(SupplyOrder)) supplyOrder = SupplyOrder;
            if (!string.IsNullOrEmpty(IdAnalitico)) idAnalitico = IdAnalitico;
            if (!string.IsNullOrEmpty(Currency)) currency = Currency;

            r = "Notificación Pre-Factura " + contract + " " + supplyOrder + " " + idAnalitico + " " + currency;
            return r;
        }
    }
}
