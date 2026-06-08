using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Api.HtmlHelpers;
using BERecepcion.Api.Infrastructure.Auth;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Copades.Dto;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Correos.Dto;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.IntegracionEFirma;
using BERecepcion.Core.Interfaces;
using BERecepcion.Core.SAPPI.Dto;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using iText.Html2pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.Copades
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class CopadeController : Controller
    {
        private readonly ICopadeRepository _copadeRepository;
        private readonly ISAPPIRepository _sapPIRepository;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly IDocumentosRepository _documentosRepository;
        private readonly ICorreoRepository _correoRepository;
        private readonly IUsuariosRepository _usuariosRepository;
        private readonly IESignRepository _eSignRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly IDocumentoFirmadoRepository _documentoFirmadoRepository;
        private readonly ICopadeServiceAsync _copadeServiceAsync;

        public CopadeController(ICopadeRepository copadeRepository, ISAPPIRepository sAPPIRepository, IHostEnvironment env, IConfiguration configuration, IDocumentosRepository documentosRepository,
            ICorreoRepository correoRepository, IUsuariosRepository usuariosRepository, IESignRepository eSignRepository, IBitacoraRepository bitacoraRepository,
            IDocumentoFirmadoRepository documentoFirmadoRepository, ICopadeServiceAsync copadeServiceAsync)
        {
            _copadeRepository = copadeRepository;
            _sapPIRepository = sAPPIRepository;
            _env = env;
            _configuration = configuration;
            _documentosRepository = documentosRepository;
            _correoRepository = correoRepository;
            _usuariosRepository = usuariosRepository;
            _eSignRepository = eSignRepository;
            _bitacoraRepository = bitacoraRepository;
            _documentoFirmadoRepository = documentoFirmadoRepository;
            _copadeServiceAsync = copadeServiceAsync;
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<DtoPdfPreviewData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListaCopadesByIdAsync([FromBody] DataResult<DtoPdfPreviewData> copadeReq)
        {
            var result = await ArmaPDF(copadeReq);
            return result.ToActionResult();
        }

        private string adjustEncoding(string c)
        {
            return c.Replace("á", "&aacute;").Replace("é", "&eacute;").Replace("í", "&iacute;").Replace("ó", "&oacute;").Replace("ú", "&uacute;").Replace("ñ", "&ntilde;").Replace("Á", "&Aacute;").Replace("É", "&Eacute;").Replace("Í", "&Iacute;").Replace("Ó", "&Oacute;").Replace("Ú", "&Uacute;").Replace("Ñ", "&Ntilde;");
        }

        private IEnumerable<CopadeDetalle> FormatDoc(IEnumerable<CopadeDetalle> copade)
        {
            for (int i1 = 0; i1 < copade.Count(); i1++)
            {
                if (copade.ElementAt(i1).Perception != "" && copade.ElementAt(i1).Deduction != "")
                {
                    decimal t1 = Convert.ToDecimal(copade.ElementAt(i1).Perception);
                    decimal t2 = Convert.ToDecimal(copade.ElementAt(i1).Deduction);

                    if (t1 == 0 && t2 == 0)
                    {
                        copade.ElementAt(i1).Perception = t1 + ".00";
                        copade.ElementAt(i1).Deduction = t2 + ".00";
                    }
                    else if (t1 == 0 || t2 == 0)
                    {
                        if (t1 == 0)
                        {
                            copade.ElementAt(i1).Perception = t1 + ".00";
                            copade.ElementAt(i1).Deduction = t2.ToString("N", new CultureInfo("en-US"));
                        }
                        else
                        {
                            copade.ElementAt(i1).Deduction = t2 + ".00";
                            copade.ElementAt(i1).Perception = t1.ToString("N", new CultureInfo("en-US"));
                        }
                    }
                    else
                    {
                        copade.ElementAt(i1).Deduction = t2.ToString("N", new CultureInfo("en-US"));
                        copade.ElementAt(i1).Perception = t1.ToString("N", new CultureInfo("en-US"));
                    }

                }
                else
                {
                    if (copade.ElementAt(i1).Perception == "")
                    {
                        copade.ElementAt(i1).Perception = "0.00";
                    }
                    else
                    {
                        copade.ElementAt(i1).Deduction = "0.00";
                    }
                    if (copade.ElementAt(i1).Concept == "Moneda" || copade.ElementAt(i1).Concept == "Moneda MXN") { copade.ElementAt(i1).Deduction = ""; copade.ElementAt(i1).Perception = ""; }
                }
            }
            return copade;
        }

        [HttpGet("GetListaFiltroCopadesAsync")]
        [ProducesResponseType(typeof(DataResult<List<CopadeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListaFiltroCopadesAsync(string UserID, int pageSize, int pageNum = 1, string search = null)
        {
            Log.Information("========== BACKEND GetListaFiltroCopadesAsync INICIO ==========");
            Log.Information($"UserID: {UserID}");
            Log.Information($"pageSize: {pageSize}");
            Log.Information($"pageNum: {pageNum}");
            Log.Information($"search: {search ?? "(null)"}");
            
            try
            {
                var result = await _copadeRepository.GetListaFiltroCopadesAsync(UserID, pageSize, pageNum, search);
                
                Log.Information($"Repositorio retornó: Status={result.Status}");
                
                if (result.Data != null)
                {
                    Log.Information($"Registros devueltos: {result.Data.Count()}");
                }
                else
                {
                    Log.Warning("result.Data es null");
                }
                
                if (!string.IsNullOrEmpty(result.Message))
                {
                    Log.Information($"Mensaje: {result.Message}");
                }
                
                var actionResult = result.ToActionResult();
                Log.Information($"ToActionResult retornó: {actionResult.GetType().Name}");
                
                return actionResult;
            }
            catch (Exception ex)
            {
                Log.Error($"========== ERROR EN BACKEND ==========");
                Log.Error($"Tipo: {ex.GetType().FullName}");
                Log.Error($"Mensaje: {ex.Message}");
                Log.Error($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Log.Error($"InnerException: {ex.InnerException.Message}");
                }
                
                throw;
            }
        }

        [HttpGet("GetPagedFiltroCopadesAsync")]
        [Authorize(Policy = PolicyConstants.RequireReceptionSignCopade)]
        public async Task<IActionResult> GetPagedFiltroCopadesAsync(
            [FromQuery] CopadeFiltroRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _copadeServiceAsync.GetPagedFiltroCopadesAsync(
                request, cancellationToken);
            return result.ToActionResult(this);
        }

        private DateTime reformatDate(string originalDate)
        {
            if (!string.IsNullOrEmpty(originalDate))
            {
                if (originalDate.Length >= 10) originalDate = originalDate.Substring(0, 10);
                if (originalDate.Contains("-"))
                {
                    string d, m, a;
                    a = originalDate.Substring(0, 4);
                    m = originalDate.Substring(5, 2);
                    d = originalDate.Substring(8, 2);
                    originalDate = d + "/" + m + "/" + a;
                }
                return Convert.ToDateTime(originalDate);
            }
            return new DateTime();
        }

        [HttpPost("Emails")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<bool> envioCorreoArchivosAsync(DtoPdfEmail copadeReq)
        {
            try
            {
                DataResult<CopadeDto> recuperaCopadeResult = new DataResult<CopadeDto>();

                recuperaCopadeResult = await _sapPIRepository.RecuperaCopadeBDAsync(copadeReq.data);

                List<string> archivos = new List<string>() { };
                string filePdf = "";

                //recuperar el documento firmado

                if (recuperaCopadeResult.Data != null)
                {

                    var documentoFirmado = await _documentoFirmadoRepository.GetDocumentoFirmadoAsync(recuperaCopadeResult.Data.CopadeID);
                    if (documentoFirmado != null)
                    {
                        var resultRecuperado = await _eSignRepository.RecuperaPDFFirmado(documentoFirmado.Data.paqueteId, documentoFirmado.Data.documentoId);
                        if (resultRecuperado.Status == System.Net.HttpStatusCode.OK)
                        {
                            byte[] pdfDataPre = Convert.FromBase64String(resultRecuperado.Data.ARCHIVO);
                            filePdf = "Files/" + recuperaCopadeResult.Data.Reception.ToString() + ".pdf";
                            FileStream fileStreamPdf = new FileStream(filePdf, FileMode.Create, FileAccess.ReadWrite);
                            fileStreamPdf.Write(pdfDataPre, 0, pdfDataPre.Length);
                            fileStreamPdf.Close();
                            archivos.Add(filePdf);
                        }
                    }

                    var xmlPreTmp = await _sapPIRepository.GetPreFacturaById(recuperaCopadeResult.Data.CopadeID);
                    var xmlNotTmp = await _sapPIRepository.GetNotaCreditoById(recuperaCopadeResult.Data.CopadeID);
                    string xmlPre = xmlPreTmp.Data;
                    string xmlNot = xmlNotTmp.Data;
                    if (!string.IsNullOrEmpty(xmlPre))
                    {
                        byte[] xmlDataPre = Encoding.Default.GetBytes(xmlPre);

                        string fileName = "Prefactura " + recuperaCopadeResult.Data.Reception.ToString();
                        string fileXml = "Files/" + fileName + ".xml";
                        FileStream fileStreamXml = new FileStream(fileXml, FileMode.Create, FileAccess.ReadWrite);
                        fileStreamXml.Write(xmlDataPre, 0, xmlDataPre.Length);
                        fileStreamXml.Close();
                        archivos.Add(fileXml);
                    }

                    if (!string.IsNullOrEmpty(xmlNot))
                    {
                        byte[] xmlDataNot = Encoding.Default.GetBytes(xmlNot);

                        string fileName = "Nota Credito " + recuperaCopadeResult.Data.Reception.ToString();
                        string fileXml = "Files/" + fileName + ".xml";
                        FileStream fileStreamXml = new FileStream(fileXml, FileMode.Create, FileAccess.ReadWrite);
                        fileStreamXml.Write(xmlDataNot, 0, xmlDataNot.Length);
                        fileStreamXml.Close();
                        archivos.Add(fileXml);
                    }
                }

                var users = await _usuariosRepository.GetUsuariosByCreditorNumber(recuperaCopadeResult.Data.CreditorNumber);

                var usersCreditorBanking = await _usuariosRepository.GetUsuariosByCreditorBanking(recuperaCopadeResult.Data.CreditorBanking);

                CorreoDto correoMensaje = new CorreoDto();
                correoMensaje.Subject = copadeReq.subject;

                DtoPdfFilesEmail dtoPdfFilesEmail = new DtoPdfFilesEmail();
                dtoPdfFilesEmail.archivos = archivos;
                dtoPdfFilesEmail.users = users.Data;
                dtoPdfFilesEmail.representatives = usersCreditorBanking.Data;
                dtoPdfFilesEmail.copade = recuperaCopadeResult.Data;
                dtoPdfFilesEmail.correoMensaje = correoMensaje;


                if (dtoPdfFilesEmail.users.ToList().Count == 0)
                {
                    //correo de supervision
                    var emailsS = _configuration["Email:CtrlBcc"];
                    if (!string.IsNullOrEmpty(emailsS))
                    {
                        var em = emailsS.Split(";");
                        List<UsersDto> u = new List<UsersDto>() { };
                        for (int i = 0; i < em.Length; i++)
                        {
                            u.Add(new UsersDto() { Email = em[i] });
                        }
                        dtoPdfFilesEmail.users = u;
                    }
                }

                var answer = await _correoRepository.EnvioCorreoArchivosAsync(dtoPdfFilesEmail);

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

                return answer;

            }
            catch (Exception ex)
            {
                Log.Error("Correos: {error}", ex.ToString());
                return false;
            }
        }

        [HttpPost("CopadeFirmaAsync/{CopadeID}")]
        [ProducesResponseType(typeof(DataResult<CopadeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CopadeFirmaAsync(Guid CopadeID, [FromBody] DataResult<CopadeDto> dto)
        {
            var result = await _copadeRepository.CopadeFirmaAsync(CopadeID, dto.User.UserType);
            return result.ToActionResult();
        }

        [HttpPost("CopadeFirmaCorreoAsync/{CopadeID}")]
        [ProducesResponseType(typeof(DataResult<CopadeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CopadeFirmaCorreoAsync(Guid CopadeID, [FromBody] DataResult<CopadeDto> dto)
        {
            var result = await _copadeRepository.CopadeFirmaCorreoAsync(CopadeID, dto.User.UserType, dto.User.Email);
            return result.ToActionResult();
        }

        [HttpPost("CopadeSigners2Async")]
        [ProducesResponseType(typeof(DataResult<ExternosDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CopadeSigners2Async([FromBody] Guid CopadeID)
        {
            var result = await _copadeRepository.GetSignersByCopadeID(CopadeID);
            return result.ToActionResult();
        }

        [HttpGet("GetCopadeByReceptionAsync")]
        [ProducesResponseType(typeof(DataResult<CopadeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCopadeByReceptionAsync(string Clave, string Reception, string Exercise)
        {
            if (string.IsNullOrEmpty(Clave) || string.IsNullOrEmpty(Reception))
            {
                return BadRequest(new { Message = "Clave y Reception son requeridos" });
            }

            PICopadeRequestDto pi = new PICopadeRequestDto() { Clave = Clave, Reception = Reception, Exercise = Exercise };
            var result = await _sapPIRepository.RecuperaCopadeBDAsync(pi);
            return result.ToActionResult();
        }
        [HttpPost("FirmaUnoAsync")]
        [ProducesResponseType(typeof(DataResult<Externos2Dto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FirmaUnoAsync([FromBody] DataResult<Externos2Dto> model)
        {

            DataResult<Externos2Dto> resultItem = new DataResult<Externos2Dto>
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso FirmaUnoAsync",
                Data = new Externos2Dto()
            };

            string NombreArchivo = string.Concat("Documento_", model.Data.paquete.Titulo.ToString(), ".pdf");

                IFormFile documentoPDF;
                PICopadeRequestDto reqItem = new PICopadeRequestDto()
                {
                    Clave = model.Data.copade.clave,
                    Exercise = model.Data.copade.Exercise,
                    Reception = model.Data.copade.Reception,
                    SapOrder = model.Data.copade.SapOrder
                };
                //PICopadeRequestDto reqList = new PICopadeRequestDto[] { reqItem };
                DtoPdfPreviewData pdfReq = new DtoPdfPreviewData()
                {
                    data = reqItem
                };

                DataResult<DtoPdfPreviewData> copadeReq = new DataResult<DtoPdfPreviewData>()
                {
                    Data = pdfReq
                };
                // Se suba el PDF Generado
                var archivoPDF = await ArmaPDF(copadeReq);


                byte[] bytes = Convert.FromBase64String(archivoPDF.Data.result.FirstOrDefault());
                string basePath = _env.ContentRootPath + "/Files/";
                bool basePathExists = Directory.Exists(basePath);
                if (!basePathExists) Directory.CreateDirectory(basePath);
                string archivo = basePath + model.Data.copade.Reception + ".pdf";

                // Se guarda el PDF en el server (temporal)
                System.IO.File.WriteAllBytes(archivo, bytes);

                using var stream = System.IO.File.OpenRead(archivo);
                
                documentoPDF = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/pdf"
                };

                //LGGD
                model.Data.paquete.IdCorrelacion = await _documentoFirmadoRepository.CreaPaqueteInicialAsync(
                    model.Data.usuario.Id.ToString(),
                    model.Data.paquete.Documentos.FirstOrDefault().CodigoTipoDocumento,
                    model.Data.usuarioBEId,
                    model.Data.documentoBEId,
                    model.Data.paquete.Firmantes.FirstOrDefault().Figura
                );

                model.Data.paquete.Documentos.FirstOrDefault().IdCorrelacion = model.Data.paquete.IdCorrelacion;
                
                DataResult<CrearPaqueteResult> paqueteResult = await _eSignRepository.PostDocumentoEFirmaAsync(
                                                                            NombreArchivo,
                                                                            model.Data.paquete, 
                                                                            documentoPDF);
                if (paqueteResult.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("No se pudo crear el paquete en EFirma: {error}", paqueteResult.Message);
                    return paqueteResult.ToActionResult();
                }

                await _documentoFirmadoRepository.ActualizaPaqueteInicialAsync(
                    paqueteResult.Data.IdCorrelacion,
                    paqueteResult.Data.IdPaquete.ToString(),
                    paqueteResult.Data.Documentos.FirstOrDefault().IdDocumento.ToString(),
                    model.Data.paquete.Firmantes.FirstOrDefault().Figura
                );

                resultItem.Data.copade = model.Data.copade;
                resultItem.Data.paqueteResult = paqueteResult.Data;
                resultItem.Data.hashOriginal = paqueteResult.Data.Documentos.FirstOrDefault().Hash;

                return Ok(resultItem);
        }

        [HttpPost("FirmaDosAsync")]
        [ProducesResponseType(typeof(DataResult<Externos2Dto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FirmaDosAsync([FromBody] DataResult<Externos2Dto> model)
        {
            DataResult<Externos2Dto> resultItem = new DataResult<Externos2Dto>()
            {                
                Status = System.Net.HttpStatusCode.OK,
                Message = "Agregar firmante dos al paquete",
                Data = new Externos2Dto()
            };
            
            model.Data.agregarFirmante.IdCorrelacion = await _documentoFirmadoRepository.CreaPaqueteInicialAsync(
                   model.Data.usuario.Id.ToString(),
                   model.Data.paquete.Documentos.FirstOrDefault().CodigoTipoDocumento,
                   model.Data.usuarioBEId,
                   model.Data.documentoBEId,
                   model.Data.paquete.Firmantes.FirstOrDefault().Figura
               );               
               
                var resultAgregaFirmante = await _eSignRepository.PostAgregaFirmanteEFirmaAsync(model.Data.agregarFirmante);
                if (resultAgregaFirmante.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("Agregar firmante: {error}", resultAgregaFirmante.Message);
                    return resultAgregaFirmante.ToActionResult();
                }

                var paqueteResult = await _eSignRepository.GetDocumentoEFirmaAsync(model.Data.paquete.IdCorrelacion);
                if (paqueteResult.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("Recuperar documento: {error}", paqueteResult.Message);
                    return paqueteResult.ToActionResult();
                }

                resultItem.Data.copade = model.Data.copade;
                resultItem.Data.paqueteResult = paqueteResult.Data;
                resultItem.Data.hashOriginal = paqueteResult.Data.Documentos.FirstOrDefault().Hash;                
                return Ok(resultItem);
        }

        [HttpPost("CompletaFirmaAsync")]
        [ProducesResponseType(typeof(DataResult<Externos2Dto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<Externos2Dto>> CompletaFirmaAsync([FromBody] DataResult<Externos2Dto> externos)
        {
            DataResult<Externos2Dto> resultItem = new DataResult<Externos2Dto>
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso",
                Data = new Externos2Dto()
            };

            DataResult<FirmarPaqueteResult> firmaPaqueteResult = await _eSignRepository.PostFirmaEFirmaAsync(externos.Data.firmaPaquete);
                if (firmaPaqueteResult.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("Externos PostDocumentoAsync eSignFirma: {error}", firmaPaqueteResult.Message);
                    resultItem.Message = firmaPaqueteResult.Message;
                    resultItem.Status = firmaPaqueteResult.Status;
                    resultItem.Data.firmaPaqueteResult = firmaPaqueteResult.Data;
                    return resultItem;
                }

                await _copadeRepository.CopadeFirmaAsync(externos.Data.copade.CopadeID, externos.User.Token);
                await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto()
                {
                    UserID = externos.Data.usuarioBEId,
                    Seccion = "Copade",
                    Accion = "Firmar",
                    Descripcion = $"Firma de Copade {externos.Data.copade.clave} {externos.Data.copade.Exercise} {externos.Data.copade.SapOrder} {externos.Data.copade.Reception}"
                });
                if (string.IsNullOrWhiteSpace(externos.Data.copade.Functionary1SignDate.ToString()))
                {
                    if (externos.Data.copade.clave == "PTRI" || externos.Data.copade.clave == "PLOG" || externos.Data.copade.clave == "PFER")
                    {                        
                        var usersCopade = await _copadeRepository.GetSigners2ByCopadeID(externos.Data.copade.CopadeID);

                        var firmante2 = await _usuariosRepository.GetUsuarioEmailAsync(usersCopade.Data.signer2, usersCopade.Data.signer2Email);
                        var suplente2 = await _usuariosRepository.GetUsuarioEmailAsync(usersCopade.Data.alternate2, usersCopade.Data.alternate2Email);

                        List<UsersDto> firmantes2List = new List<UsersDto>();
                        if (firmante2.Data != null)
                            firmantes2List.Add(firmante2.Data);

                        if (suplente2.Data != null)
                            firmantes2List.Add(suplente2.Data);

                        var listaCorreos = !string.IsNullOrWhiteSpace(firmante2.Data.Email) ? firmante2.Data.Email : "";
                        listaCorreos += !string.IsNullOrWhiteSpace(suplente2.Data.Email) ? $",{suplente2.Data.Email}" : "";

                        var mail = await _correoRepository.NotificacionCOPADEAsync(firmantes2List, externos.Data.copade, "Firma de Copade");
                        await _copadeRepository.CopadeFirmaCorreoAsync(externos.Data.copade.CopadeID, listaCorreos, "2");

                        firmaPaqueteResult.Status = mail ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Conflict;
                    }
                    else if (externos.Data.copade.clave == "PEP" || externos.Data.copade.clave == "PCORP")
                    {                        
                        // Enviar correo a Proveedor Attach(creditorNumber)
                        DtoPdfEmail dtoPdfEmail = new DtoPdfEmail();
                        dtoPdfEmail.data = new PICopadeRequestDto() { Clave = externos.Data.copade.clave, Exercise = externos.Data.copade.Exercise, Reception = externos.Data.copade.Reception, SapOrder = externos.Data.copade.SapOrder };
                        dtoPdfEmail.subject = setCompletaFirmaSubject(externos.Data.copade.Contract, externos.Data.copade.SapOrder, externos.Data.copade.Reception, externos.Data.copade.Currency);
                        dtoPdfEmail.result = "";
                        dtoPdfEmail.UsuarioModificador = externos.User.UserID.ToString();
                        await envioCorreoArchivosAsync(dtoPdfEmail);
                        // registramos evento de correo
                        var users = await _usuariosRepository.GetUsuariosByCreditorNumber(externos.Data.copade.CreditorNumber);
                        var listaCorreos = "";
                        foreach (var u in users.Data)
                        {
                            listaCorreos += $"{u.Email},";
                        }
                        // Registramos el correo del proveedor al que se le manda el correo de pre factura
                        await _copadeRepository.CopadeFirmaCorreoAsync(externos.Data.copade.CopadeID, listaCorreos, "P");

                    }
                }
                else if (!string.IsNullOrWhiteSpace(externos.Data.copade.Functionary1SignDate.ToString()))
                {
                    // Enviar correo a Proveedor Attach(creditorNumber)
                    DtoPdfEmail dtoPdfEmail = new DtoPdfEmail();
                    dtoPdfEmail.data = new PICopadeRequestDto() { Clave = externos.Data.copade.clave, Exercise = externos.Data.copade.Exercise, Reception = externos.Data.copade.Reception, SapOrder = externos.Data.copade.SapOrder };
                    dtoPdfEmail.subject = setCompletaFirmaSubject(externos.Data.copade.Contract, externos.Data.copade.SapOrder, externos.Data.copade.Reception, externos.Data.copade.Currency);
                    dtoPdfEmail.result = "";
                    dtoPdfEmail.UsuarioModificador = externos.User.UserID.ToString();
                    await envioCorreoArchivosAsync(dtoPdfEmail);
                    // registramos evento de correo

                    var users = await _usuariosRepository.GetUsuariosByCreditorNumber(externos.Data.copade.CreditorNumber);
                    var listaCorreos = "";
                    foreach (var u in users.Data)
                    {
                        listaCorreos += $"{u.Email},";
                    }
                    await _copadeRepository.CopadeFirmaCorreoAsync(externos.Data.copade.CopadeID, listaCorreos, "P");
                }
                resultItem.Data.firmaPaqueteResult = firmaPaqueteResult.Data;
                return resultItem;
        }

        private async Task<DataResult<DtoPdfPreviewData>> ArmaPDF(DataResult<DtoPdfPreviewData> copadeReq)
        {
            List<string> result = new List<string>() { };
            if (copadeReq == null) return null;

            DataResult<CopadeDto> recuperaCopadeResult = new DataResult<CopadeDto>();
            if (copadeReq.Data.data.Clave.Equals("PCORP"))
            {
                recuperaCopadeResult = await _sapPIRepository.RecuperaCopadeBDAsync(copadeReq.Data.data);

            }
            else // PEP o PTRI
            {
                recuperaCopadeResult = await _sapPIRepository.RecuperaCopadeAsync(copadeReq.Data.data);

                if (recuperaCopadeResult.Data != null)
                {
                    recuperaCopadeResult.Data.CopadeID = await _sapPIRepository.GetCopadeIdAsync(copadeReq.Data.data);
                    copadeReq.Data.data.Exercise = recuperaCopadeResult.Data.Exercise;

                    // para guardar el copade en la BD                                               
                    var tmp = await _sapPIRepository.InsertaCopadeAsync(recuperaCopadeResult.Data);
                    recuperaCopadeResult.Data.LogoB64 = await _sapPIRepository.GetLogo(copadeReq.Data.data.Clave);
                }

            }

            if (recuperaCopadeResult.Status == System.Net.HttpStatusCode.OK)
            {
                if (recuperaCopadeResult.Data != null)
                {
                    CopadeDto copade = recuperaCopadeResult.Data;
                    string viewIndex = "";
                    if (copade != null)
                    {
                        switch (copade.clave)
                        {
                            case "PTRI":
                            case "PFER":
                            case "PLOG":
                                copade.vDetalle = FormatDoc(copade.vDetalle);
                                copade.vDetalleAvion = FormatDoc(copade.vDetalleAvion);
                                copade.vDetalleObra.Detalle = FormatDoc(copade.vDetalleObra.Detalle);

                                viewIndex = "~/Views/PTRI/IndexPtri.cshtml";
                                break;
                            case "PEP":
                                viewIndex = "~/Views/PEP/IndexPep.cshtml";
                                break;
                            case "PCORP":
                                viewIndex = "~/Views/PCORP/IndexPCorp.cshtml";
                                break;
                        }

                        string renderized = adjustEncoding(this.RenderViewAsync(viewIndex, copade, false).Result);

                        byte[] applicationPDFData = Encoding.ASCII.GetBytes(renderized);
                        string archivo = Guid.NewGuid().ToString();
                        //string filePath = "Files/" + archivo + ".html";
                        string filePath = String.Concat("Files/",archivo,".html");
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

                        result.Add(Convert.ToBase64String(bytes));
                    }
                }
            }

            DataResult<DtoPdfPreviewData> dataResult = new DataResult<DtoPdfPreviewData>();
            dataResult.Data = new DtoPdfPreviewData();
            dataResult.Data.data = copadeReq.Data.data;
            dataResult.Data.result = result;
            dataResult.Data.UsuarioModificador = copadeReq.Data.UsuarioModificador;
            dataResult.Message = result.Count == 0 ? "Error en la generacion de archivo" : "Generacion exitosa de archivos";
            dataResult.Status = System.Net.HttpStatusCode.OK;

            return dataResult;
        }

        private string setCompletaFirmaSubject(string Contract, string SupplyOrder, string Reception, string Currency)
        {
            string r, contract, supplyOrder, reception, currency;
            r = contract = supplyOrder = reception = currency = "";

            if (!string.IsNullOrEmpty(Contract)) contract = Contract;
            if (!string.IsNullOrEmpty(SupplyOrder)) supplyOrder = SupplyOrder;
            if (!string.IsNullOrEmpty(Reception)) reception = Reception;
            if (!string.IsNullOrEmpty(Currency)) currency = Currency;

            r = "Notificación Pre-Factura " + contract + " " + supplyOrder + " " + reception + " " + currency;
            return r;
        }
    }
}
