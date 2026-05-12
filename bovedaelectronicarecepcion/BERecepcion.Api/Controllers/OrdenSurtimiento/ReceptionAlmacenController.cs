using BERecepcion.Api.Filters;
using BERecepcion.Api.HtmlHelpers;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Dto;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using iText.Html2pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BERecepcion.Core.IntegracionEFirma;
using System.Linq;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Infraestructura.Admin.Repositories;
using BERecepcion.Infraestructura.Correos.Repositories;
using BERecepcion.Core.Admin.Dto;

namespace BERecepcion.Api.Controllers.OrdenSurtimiento
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class ReceptionAlmacenController : Controller
    {
        private readonly IReceptionAlmacenRepository _receptionAlmacenRepository;
        private readonly ISAPPIRepository _sapPIRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly IDocumentoFirmadoRepository _documentoFirmadoRepository;
        private readonly IESignRepository _eSignRepository;
        private readonly IConfiguration _configuration;
        private readonly IDocumentosRepository _documentosRepository;
        private readonly IHostEnvironment _env;
        private readonly ICorreoRepository _correoRepository;
        private readonly IUsuariosRepository _usuariosRepository;

        public ReceptionAlmacenController(IReceptionAlmacenRepository receptionAlmacenRepository, ISAPPIRepository sAPPIRepository, 
                                          IBitacoraRepository bitacoraRepository, IDocumentoFirmadoRepository documentoFirmadoRepository, 
                                          IESignRepository eSignRepository, IConfiguration configuration, 
                                          IDocumentosRepository documentosRepository, IHostEnvironment env,
                                          IUsuariosRepository usuariosRepository, ICorreoRepository correoRepository)
        {
            _receptionAlmacenRepository = receptionAlmacenRepository;
            _sapPIRepository = sAPPIRepository;
            _bitacoraRepository = bitacoraRepository;
            _documentoFirmadoRepository = documentoFirmadoRepository;
            _eSignRepository = eSignRepository;
            _configuration = configuration;
            _documentosRepository = documentosRepository;
            _env = env;
            _usuariosRepository = usuariosRepository;
            _correoRepository = correoRepository;
        }

        [HttpGet("GetReceptionAsync")]
        [ProducesResponseType(typeof(IEnumerable<ReceptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReceptionAsync(string Token, int pageSize, int pageNum = 1, string search = null)
        {
            try
            {
                return Ok(await _receptionAlmacenRepository.GetReceptionAsync(Token, pageSize, pageNum, search));
            }
            catch (Exception ex)
            {
                Log.Error("GetOSInternoAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetRecuperaReceptionAsync/{receptionId}")]
        [ProducesResponseType(typeof(ReceptionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecuperaReceptionAsync(Guid receptionId)
        {
            try
            {
                return Ok(await _receptionAlmacenRepository.RecuperaReceptionAsync(receptionId));
            }
            catch (Exception ex)
            {
                Log.Error("GetReceptionAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetReceptionPDFAsync")]
        [ProducesResponseType(typeof(DataResult<ArchivoPDFDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReceptionPDFAsync(Guid ReceptionId)
        {
            try
            {
                return Ok(await ArmaPDFAsync(ReceptionId));
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        [HttpPost("FirmaAsync")]
        [ProducesResponseType(typeof(DataResult<Externos2Dto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FirmaAsync([FromBody] DataResult<Externos2Dto> model)
        {
            DataResult<Externos2Dto> resultItem = new DataResult<Externos2Dto>
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Recepción en almacen exitosa FirmaUnoAsync",
                Data = new Externos2Dto()
            };
            try
            {
                string NombreArchivo = string.Concat("Documento_", model.Data.paquete.Titulo.ToString(), ".pdf");

                DataResult<ArchivoPDFDto> archivoPDF = new DataResult<ArchivoPDFDto>();
                string archivo = "";
                IFormFile documentoPDF;
                Byte[] bytes;
                string basePath = _env.ContentRootPath + "/Files/";
                bool basePathExists = Directory.Exists(basePath);
                if (!basePathExists) Directory.CreateDirectory(basePath);

                archivoPDF = await ArmaPDFAsync(model.Data.reception.ReceptionID);
                if (archivoPDF.Status != System.Net.HttpStatusCode.OK)
                {
                    archivo = basePath + "prueba.pdf";
                }
                else
                {
                    bytes = Convert.FromBase64String(archivoPDF.Data.ARCHIVO);
                    archivo = basePath + model.Data.supplyOrder.SAPOrder + ".pdf";
                    System.IO.File.WriteAllBytes(archivo, bytes);
                }

                using var stream = System.IO.File.OpenRead(archivo);

                documentoPDF = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/pdf"
                };

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
                    return Problem(null, null, 500, "Error interno", null);
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
            catch (Exception ex)
            {
                Log.Error("ReceptionAlmacen FirmaAsync: {error}", ex.Message);
                return Problem(null, null, 500, "Error interno", null);
            }
        }
        [HttpPost("CompletaFirmaAsync")]
        [ProducesResponseType(typeof(DataResult<ExternosDto>), StatusCodes.Status200OK)]
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
            try
            {
                var REResponseDto = new REResponseDto
                {
                    item = new REResponseItemDto
                    {
                        Reception = externos.Data.reception.Reception,
                        Exercise = externos.Data.reception.Exercise
                    }
                };
                // cambio de estatus a la siguiente firma no borrar
                //var sapPI = await _sapPIRepository.PostRE_Response(REResponseDto);

                //if (sapPI.Data.Status.Equals("ACEPTADO"))
                //{
                // cambiamos estatus a la siguiente firma
                await _receptionAlmacenRepository.ReceptionFirmaAsync(externos.Data.reception.ReceptionID);
                // escribimos en bitacora
                await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto()
                {
                    UserID = externos.Data.usuarioBEId,
                    Seccion = "Recepción en almacén",
                    Accion = "Firmar",
                    Descripcion = $"Firma de Recepción en almacén {externos.Data.paymentList.ListaPago_Id}"
                });


                //await _bitacoraRepository.InsertaBitacoraAsync(externos.Data.documentoFirmaBE.Bitacora);
                //}

                // resultItem = eSignFirma;
                return resultItem;
            }
            catch (Exception ex)
            {
                Log.Error("EstimacionObra CompletaFirmaAsync: {error}", ex.Message);
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador";
                return resultItem;
            }
        }

        private async Task<DataResult<ArchivoPDFDto>> ArmaPDFAsync(Guid ReceptionId)
        {
            DataResult<ArchivoPDFDto> archivoPDF = new DataResult<ArchivoPDFDto> { Data = new ArchivoPDFDto(), Status = System.Net.HttpStatusCode.OK };
            try
            {
                string viewIndex = "~/Views/Reception/PCORP/Index.cshtml";
                ReceptionDto receptionDto = await _receptionAlmacenRepository.RecuperaReceptionAsync(ReceptionId);
                string renderized = adjustEncoding(this.RenderViewAsync(viewIndex, receptionDto, false).Result);
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
                if (System.IO.File.Exists(@"files/" + archivo + ".html"))
                    System.IO.File.Delete(@"files/" + archivo + ".html");
                if (System.IO.File.Exists(@"files/" + archivo + ".pdf"))
                    System.IO.File.Delete(@"files/" + archivo + ".pdf");
                archivoPDF.Data.ARCHIVO = Convert.ToBase64String(bytes);
                return archivoPDF;
            }
            catch (Exception)
            {
                archivoPDF.Status = System.Net.HttpStatusCode.BadRequest;
                return archivoPDF;
            }
        }
        private string adjustEncoding(string c)
        {
            return c.Replace("á", "&aacute;").Replace("é", "&eacute;").Replace("í", "&iacute;").Replace("ó", "&oacute;").Replace("ú", "&uacute;").Replace("ñ", "&ntilde;").Replace("Á", "&Aacute;").Replace("É", "&Eacute;").Replace("Í", "&Iacute;").Replace("Ó", "&Oacute;").Replace("Ú", "&Uacute;").Replace("Ñ", "&Ntilde;");
        }
    }
}
