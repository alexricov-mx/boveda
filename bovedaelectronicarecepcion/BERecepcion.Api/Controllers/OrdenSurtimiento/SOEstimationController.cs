using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Api.ModelBinding;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.Estimaciones.Dtos;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.IntegracionEFirma;
using BERecepcion.Core.Interfaces;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Dto;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.OrdenSurtimiento
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class SOEstimationController : ControllerBase
    {
        private readonly ISOEstimationRepository _SOEstimationRepository;
        private readonly IHostEnvironment _env;
        private readonly IESignRepository _eSignRepository;
        private readonly ISAPPIRepository _sAPPIRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly ICorreoRepository _correoRepository;
        private readonly IUsuariosRepository _usuariosRepository;
        private readonly IDocumentosRepository _documentosRepository;
        private readonly IDocumentoFirmadoRepository _documentoFirmadoRepository;
        private readonly ISoEstimacionServiceAsync _soEstimacionService;

        public SOEstimationController(ISOEstimationRepository sOEstimationRepository, IConfiguration configuration,
                                      IHostEnvironment env, IESignRepository eSignRepository,
                                      ISAPPIRepository sAPPIRepository, IBitacoraRepository bitacoraRepository,
                                      IUsuariosRepository usuariosRepository, ICorreoRepository correoRepository,
                                      IDocumentosRepository documentosRepository,
                                      IDocumentoFirmadoRepository documentoFirmadoRepository,
                                      ISoEstimacionServiceAsync soEstimacionService)
        {
            _SOEstimationRepository = sOEstimationRepository;
            _env = env;
            _eSignRepository = eSignRepository;
            _sAPPIRepository = sAPPIRepository;
            _bitacoraRepository = bitacoraRepository;
            _usuariosRepository = usuariosRepository;
            _correoRepository = correoRepository;
            _documentosRepository = documentosRepository;
            _documentoFirmadoRepository = documentoFirmadoRepository;
            _soEstimacionService = soEstimacionService;
        }

        [HttpGet("{Contract}")]
        [ProducesResponseType(typeof(IEnumerable<SOEstimationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetSOEstimacion(string Contract)
        {
            try
            {
                return Ok(await _SOEstimationRepository.GetSOEstimacionAsync(Contract));
            }
            catch (Exception ex)
            {
                Log.Error("GetSOEstimacion: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }


        [HttpGet("GetSOEInternoAsync")]
        [ProducesResponseType(typeof(IEnumerable<SOEstimationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSOEInternoAsync(string Token, int pageSize, int pageNum = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _SOEstimationRepository.GetSOEInternoAsync(Token, pageSize, pageNum);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("GetSOEInternoAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        /// <summary>
        /// Obtiene estimaciones internas paginadas usando la capa de servicio con Result&lt;T&gt;.
        /// Las respuestas de error siguen ProblemDetails RFC 7807.
        /// </summary>
        [HttpGet("GetSupplyOrderEstimacionesInternoAsync")]
        [ProducesResponseType(typeof(PagedResult<SOEstimationInternoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSupplyOrderEstimacionesInternoAsync(
            [FromQuery] SOEstimationInternoRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _soEstimacionService
                .GetSOEInternoPaginationAsync(request, cancellationToken);

            return result.ToActionResult(this);
        }


        [HttpGet("GetSOEProveedorAsync")]
        [ProducesResponseType(typeof(IEnumerable<SOEstimationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSOEProveedorAsync(string CreditorNumber, int pageSize, int pageNum = 1)
        {
            try
            {
                return Ok(await _SOEstimationRepository.GetSOEProveedorAsync(CreditorNumber, pageSize, pageNum));
            }
            catch (Exception ex)
            {
                Log.Error("GetSOEProveedorAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
        [HttpGet("GetSOEProveedorPaginatorAsync")]
        public async Task<IActionResult> GetSOEProveedorPaginatorAsync(
            [FromQuery] SOEstimationProveedorRequestDto request,
            CancellationToken cancellationToken = default
            )
        {
            var result = await _soEstimacionService.GetSOEProveedorPaginationAsync(
                request, cancellationToken
                ); 
            return result.ToActionResult(this);
        }

        [HttpPost("Firma2")]
        [ProducesResponseType(typeof(DataResult<ExternosDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostFirma2Async([FromForm] BovedaModelWrapper model)
        {
            DataResult<ExternosDto> resultItem = new DataResult<ExternosDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso PostFirma2Async"
            };
            // Agregamos al 2do firmante
            // Dto paqueteFirmantes
            var resultAgregaFirmante = await _eSignRepository.PostAgregaFirmanteAsync(model.Externos);
            if (resultAgregaFirmante.Status == System.Net.HttpStatusCode.OK)
            {
                // firma en eSign
                // Dto documentoFirma
                var eSignFirma = await _eSignRepository.PostFirmaAsync(model.Externos, "2");
                if (eSignFirma.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("Externos PostFirma2Async eSignFirma: {error}", eSignFirma.Message);
                    return Problem(null, null, 500, "Error interno", null);
                }

            }
            return Ok(resultItem);
        }

        [HttpGet("GetSOEstimacionFiltroAsync")]
        [ProducesResponseType(typeof(DataResult<List<CopadeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSOEstimacionFiltroAsync(string Token, string Filtro)
        {
            try
            {
                return Ok(await _SOEstimationRepository.GetSOEstimacionFiltroAsync(Token, Filtro));
            }
            catch (Exception ex)
            {
                Log.Error("Perfiles: GetPerfilAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        #region Firma de estimacion de obra
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

            try
            {
                string NombreArchivo = string.Concat("Documento_", model.Data.paquete.Titulo.ToString(), ".pdf");

                DataResult<ArchivoPDFDto> archivoPDF = new DataResult<ArchivoPDFDto>();
                string archivo = "";
                IFormFile documentoPDF;
                Byte[] bytes;
                string basePath = _env.ContentRootPath + "/Files/";
                bool basePathExists = System.IO.Directory.Exists(basePath);
                if (!basePathExists) Directory.CreateDirectory(basePath);

                archivoPDF = await _documentosRepository.GetDocumentoAsync(model.Data.sOEstimation.SapOrder,
                                                                           model.Data.sOEstimation.OrganismClave);
                if (archivoPDF.Status != System.Net.HttpStatusCode.OK)
                {
                    archivo = basePath + "prueba.pdf";
                }
                else
                {
                    bytes = Convert.FromBase64String(archivoPDF.Data.ARCHIVO);
                    archivo = basePath + model.Data.sOEstimation.SapOrder + ".pdf";
                    System.IO.File.WriteAllBytes(archivo, bytes);
                }

                using var stream = System.IO.File.OpenRead(archivo);


                documentoPDF = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name))
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
                //LGGD

            }
            catch (Exception ex)
            {
                Log.Error("Estimacion Obra FirmaUnoAsync: {error}", ex.Message);
                return Problem(null, null, 500, "Error interno", null);
            }
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

            try
            {
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
                    resultItem.Status = System.Net.HttpStatusCode.InternalServerError;
                    resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador";
                    return BadRequest(resultItem);
                }

                var paqueteResult = await _eSignRepository.GetDocumentoEFirmaAsync(model.Data.paquete.IdCorrelacion);
                if (paqueteResult.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("Recuperar documento: {error}", paqueteResult.Message);
                    resultItem.Status = System.Net.HttpStatusCode.NotFound;
                    resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador";
                    return BadRequest(resultItem);
                }

                resultItem.Data.sOEstimation = model.Data.sOEstimation;
                resultItem.Data.paqueteResult = paqueteResult.Data;
                resultItem.Data.hashOriginal = paqueteResult.Data.Documentos.FirstOrDefault().Hash;
                return Ok(resultItem);
            }
            catch (Exception ex)
            {
                Log.Error("Estimación de obra PostFirma2Async: {error}", ex.Message);
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador";
                return BadRequest(resultItem);
            }
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
            try
            {
                DataResult<FirmarPaqueteResult> firmaPaqueteResult = await _eSignRepository.PostFirmaEFirmaAsync(externos.Data.firmaPaquete);
                if (firmaPaqueteResult.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("Externos PostDocumentoAsync eSignFirma: {error}", firmaPaqueteResult.Message);
                    resultItem.Message = firmaPaqueteResult.Message;
                    resultItem.Status = firmaPaqueteResult.Status;
                    resultItem.Data.firmaPaqueteResult = firmaPaqueteResult.Data;
                    return resultItem;
                }

                var responseDto = new OSResponseDto
                {
                    item = new OSResponseItemDto
                    {
                        CONTRATO = externos.Data.sOEstimation.Contract,
                        ORDEN_SAP = externos.Data.sOEstimation.SapOrder,
                        ORGANISMO = externos.Data.sOEstimation.OrganismClave,
                        TIPO = string.IsNullOrWhiteSpace(externos.Data.sOEstimation.ProviderSignDate.ToString()) ? "G" : "S"
                    }
                };
                // cambio de estatus a la siguiente firma
                var sapPI = await _sAPPIRepository.PostOS_Response(responseDto);

                if (sapPI.Status != System.Net.HttpStatusCode.OK)
                {
                    // Se envio NO exitoso a SAP
                    // escribir en bitacora que no se pudo notificar a SAP
                    await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                    {
                        Accion = "Notificacion SAP",
                        Descripcion = $"Error al enviar cambio de status Estimación de Obra {externos.Data.sOEstimation.SapOrder}",
                        Seccion = "EstimacionObra",
                        UserID = externos.User.UserID
                    });
                }
                else
                {
                    // Se registra un envio exitoso a SAP
                    await _SOEstimationRepository.EstimationFirmaNotificacionAsync(externos.Data.sOEstimation.EstimacionID, externos.User.UserType);
                    await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                    {
                        Accion = "Notificacion SAP",
                        Descripcion = $"Envio exitoso de la Estimación de Obra {externos.Data.sOEstimation.SapOrder}",
                        Seccion = "EstimacionObra",
                        UserID = externos.User.UserID
                    });
                }
                // cambiamos estatus a la siguiente firma
                await _SOEstimationRepository.EstimationFirmaAsync(externos.Data.sOEstimation.EstimacionID, externos.User.UserType);
                // escribimos en bitacora
                await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                {
                    Accion = "Cambio de Estado",
                    Descripcion = $"Envio a Firma {externos.Data.sOEstimation.SapOrder}",
                    Seccion = "EstimacionObra",
                    UserID = externos.User.UserID
                });
                if (string.IsNullOrWhiteSpace(externos.Data.sOEstimation.ProviderSignDate.ToString()))
                {
                    // mandamos correo
                    var users = await _usuariosRepository.GetUsuariosByToken(externos.Data.sOEstimation.Signer);
                    var correosUsuariosFicha = "";
                    foreach (var item in users.Data)
                    {
                        correosUsuariosFicha += item.Email + ";";
                    }

                    var usersRepresentative = await _usuariosRepository.GetUsuariosByCreditorBanking(externos.Data.sOEstimation.Representative);
                    var mail = await _correoRepository.NotificacionESAsync(users.Data, usersRepresentative.Data, externos.Data.sOEstimation, "Firma de Estimación de obra");
                    if (mail)
                    {
                        await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                        {
                            Accion = "Envio correo",
                            Descripcion = $"Envio exitoso de correo de Estimación de Obra {externos.Data.sOEstimation.SapOrder} - Correos = {correosUsuariosFicha} - Ficha = {externos.Data.sOEstimation.Signer}",
                            Seccion = "EstimacionObra",
                            UserID = externos.User.UserID
                        });
                        // registramos evento de correo  
                        await _SOEstimationRepository.EstimationFirmaCorreoAsync(externos.Data.sOEstimation.EstimacionID, users.User.UserType, correosUsuariosFicha);
                    }
                    else
                    {
                        await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                        {
                            Accion = "Envio correo",
                            Descripcion = $"Error al enviar correo de Estimación de Obra {externos.Data.sOEstimation.SapOrder}",
                            Seccion = "EstimacionObra",
                            UserID = externos.User.UserID
                        });
                    }
                    firmaPaqueteResult.Status = mail ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Conflict;
                }

                resultItem.Data.firmaPaqueteResult = firmaPaqueteResult.Data;
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


        [HttpPost("EstimationFirmaAsync")]
        [ProducesResponseType(typeof(DataResult<SOEstimationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EstimationFirmaAsync([FromBody] DataResult<SOEstimationDto> dto)
        {
            try
            {
                return Ok(await _SOEstimationRepository.EstimationFirmaAsync(dto.Data.EstimacionID, dto.User.UserType));
            }
            catch (Exception ex)
            {
                Log.Error("EstimationFirmaAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("EstimationFirmaCorreoAsync")]
        [ProducesResponseType(typeof(DataResult<SOEstimationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EstimationFirmaCorreoAsync([FromBody] DataResult<SOEstimationDto> dto)
        {
            try
            {
                return Ok(await _SOEstimationRepository.EstimationFirmaCorreoAsync(dto.Data.EstimacionID, dto.User.UserType, dto.User.Email));
            }
            catch (Exception ex)
            {
                Log.Error("EstimationFirmaCorreoAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        #endregion

        #region Consulta
        [HttpGet("Consulta/GetESAsync")]
        [ProducesResponseType(typeof(IEnumerable<SOEstimationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetESAsync(int pageSize, Guid userId, DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1, string search = null, bool esDescarga = false)
        {
            try
            {
                return Ok(await _SOEstimationRepository.GetESAsync(pageSize, userId, pageNum, fechaInicial, fechaFinal, search, esDescarga));
            }
            catch (Exception ex)
            {
                Log.Error("GetOSAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        #endregion
    }
}
