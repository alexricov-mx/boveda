using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.Models;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Dto;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using BERecepcion.Core.IntegracionEFirma;
using BERecepcion.Infraestructura.FirmaDocumentos.Repositories;
using BERecepcion.Infraestructura.OrdenSurtimiento.Repositories;

// LGGD en proceso

namespace BERecepcion.Api.Controllers.OrdenSurtimiento
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class SupplyOrdersController : ControllerBase
    {
        private readonly ISupplyOrderRepository _supplyOrderRepository;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;
        private readonly IESignRepository _eSignRepository;
        private readonly ISAPPIRepository _sAPPIRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly ICorreoRepository _correoRepository;
        private readonly IUsuariosRepository _usuariosRepository;
        private readonly IDocumentoFirmadoRepository _documentoFirmadoRepository;
        private readonly IDocumentosRepository _documentosRepository;

        public SupplyOrdersController(ISupplyOrderRepository supplyOrderRepository, IConfiguration configuration, 
                                      IHostEnvironment env, IESignRepository eSignRepository, 
                                      ISAPPIRepository sAPPIRepository, IBitacoraRepository bitacoraRepository, 
                                      IUsuariosRepository usuariosRepository, ICorreoRepository correoRepository, 
                                      IDocumentoFirmadoRepository documentoFirmadoRepository, 
                                      IDocumentosRepository documentosRepository)
        {
            _supplyOrderRepository = supplyOrderRepository;
            _configuration = configuration;
            _env = env;
            _eSignRepository = eSignRepository;
            _sAPPIRepository = sAPPIRepository;
            _bitacoraRepository = bitacoraRepository;
            _usuariosRepository = usuariosRepository;
            _correoRepository = correoRepository;
            _documentoFirmadoRepository = documentoFirmadoRepository;
            _documentosRepository = documentosRepository;
        }
        // GET: api/SupplyOrders/GetOSInternoAsync

        #region Ordenes de surtimiento
        [HttpGet("GetOSInternoAsync")]
        [ProducesResponseType(typeof(IEnumerable<SupplyOrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOSInternoAsync(string Token, int pageSize, string search, int pageNum = 1)
        {
            try
            {
                return Ok(await _supplyOrderRepository.GetOSInternoAsync(Token, pageSize, search, pageNum));
            }
            catch (Exception ex)
            {
                Log.Error("GetOSInternoAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
        // GET: api/SupplyOrders/GetOSProveedorAsync
        [HttpGet("GetOSProveedorAsync")]
        [ProducesResponseType(typeof(IEnumerable<SupplyOrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOSProveedorAsync(string CreditorNumber, int pageSize, string search = null, int pageNum = 1)
        {
            try
            {
                return Ok(await _supplyOrderRepository.GetOSProveedorAsync(CreditorNumber, pageSize, search, pageNum));
            }
            catch (Exception ex)
            {
                Log.Error("GetOSProveedorAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
        [HttpPost("SupplyOrderFirmaAsync")]
        [ProducesResponseType(typeof(DataResult<SupplyOrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SupplyOrderFirmaAsync([FromBody] DataResult<SupplyOrderDto> dto)
        {
            try
            {
                return Ok(await _supplyOrderRepository.SupplyOrderFirmaAsync(dto.Data.SupplyOrderID, dto.User.UserType, dto.User.Token));
            }
            catch (Exception ex)
            {
                Log.Error("SupplyOrderFirmaAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("SupplyOrderFirmaCorreoAsync")]
        [ProducesResponseType(typeof(DataResult<SupplyOrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SupplyOrderFirmaCorreoAsync([FromBody] DataResult<SupplyOrderDto> dto)
        {
            try
            {
                return Ok(await _supplyOrderRepository.SupplyOrderFirmaCorreoAsync(dto.Data.SupplyOrderID, dto.User.UserType, dto.User.Email));
            }
            catch (Exception ex)
            {
                Log.Error("SupplyOrderFirmaCorreoAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
        #endregion

        #region Firma de ordenes de surtimiento
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

                archivoPDF = await _documentosRepository.GetDocumentoAsync(model.Data.supplyOrder.SAPOrder, 
                                                                           model.Data.supplyOrder.OrganismClave);
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

                using (var stream = System.IO.File.OpenRead(archivo))
                {
                    documentoPDF = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "application/pdf"
                    };

                    model.Data.paquete.IdCorrelacion = await _documentoFirmadoRepository.CreaPaqueteInicialAsync
                    (
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
                    resultItem.Data.sOEstimation = model.Data.sOEstimation;
                    resultItem.Data.paqueteResult = paqueteResult.Data;
                    resultItem.Data.hashOriginal = paqueteResult.Data.Documentos.FirstOrDefault().Hash;
                }
                return Ok(resultItem);
            }
            catch (Exception ex)
            {
                Log.Error("SupplyOrders FirmaUnoAsync: {error}", ex.Message);
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
                    //Regresa status BadRequest
                    Log.Error("SupplyOrders FirmaDosAsync: {error}", resultAgregaFirmante.Message);
                    resultItem.Status = System.Net.HttpStatusCode.BadRequest;
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
                Log.Error("SupplyOrders PostFirma2Async: {error}", ex.Message);
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador";
                return BadRequest(resultItem);
            }
        }

        [HttpPost("CompletaFirmaAsync")]
        [ProducesResponseType(typeof(DataResult<Externos2Dto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompletaFirmaAsync([FromBody] DataResult<Externos2Dto> externos)
        {
            DataResult<Externos2Dto> resultItem = new DataResult<Externos2Dto>
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso"
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
                    return BadRequest(resultItem);
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
                        Descripcion = $"Error al enviar cambio de status Orden Surtimiento {externos.Data.supplyOrder.SAPOrder}",
                        Seccion = "OrdenSurtimiento",
                        UserID = externos.User.UserID
                    });
                }
                else
                {
                    // Se registra un envio exitoso a SAP
                    await _supplyOrderRepository.SupplyOrderFirmaNotificacionAsync(externos.Data.supplyOrder.SupplyOrderID, externos.User.UserType);
                    await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                    {
                        Accion = "Notificacion SAP",
                        Descripcion = $"Envio exitoso de la Orden Surtimiento {externos.Data.supplyOrder.SAPOrder}",
                        Seccion = "OrdenSurtimiento",
                        UserID = externos.User.UserID
                    });
                }
                // cambiamos estatus a la siguiente firma
                await _supplyOrderRepository.SupplyOrderFirmaAsync(externos.Data.supplyOrder.SupplyOrderID, externos.User.UserType, 
                                                                   externos.User.Token);
                // escribimos en bitacora que se realizo la accion de firma
                await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                {
                    Accion = "Cambio de Estado",
                    Descripcion = $"Envio a Firma {externos.Data.sOEstimation.SapOrder}", //LGGD pendiente de detallar
                    Seccion = "EstimacionObra",
                    UserID = externos.User.UserID
                });
                if (string.IsNullOrWhiteSpace(externos.Data.supplyOrder.FunctionarySignDate.ToString()))
                {
                    // obtenemos correo del proveedor
                    var users = await _usuariosRepository.GetUsuariosByCreditorNumber(externos.Data.supplyOrder.CreditorNumber);

                    var correosUsuariosProv = "";
                    foreach (var item in users.Data)
                    {
                        correosUsuariosProv += item.Email + ";";
                    }

                    //envio del correo
                    var mail = await _correoRepository.NotificacionOSAsync(users.Data, externos.Data.supplyOrder, "Firma de Órden de Surtimiento");
                    if (mail) 
                    {
                        await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                        {
                            Accion = "Envio correo",
                            Descripcion = $"Envio exitoso de correo de Orden Surtimiento {externos.Data.supplyOrder.SAPOrder} - Correos = {correosUsuariosProv} - CreditorNumber = {externos.Data.supplyOrder.CreditorNumber}",
                            Seccion = "OrdenSurtimiento",
                            UserID = externos.User.UserID
                        });
                        // registramos evento de correo                    
                        await _supplyOrderRepository.SupplyOrderFirmaCorreoAsync(externos.Data.supplyOrder.SupplyOrderID, "UserTypeP", correosUsuariosProv);
                    }
                    else
                    {
                        await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                        {
                            Accion = "Envio correo",
                            Descripcion = $"Error al enviar correo de Orden Surtimiento {externos.Data.supplyOrder.SAPOrder}",
                            Seccion = "OrdenSurtimiento",
                            UserID = externos.User.UserID
                        });
                    }
                    firmaPaqueteResult.Status = mail ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Conflict;
                }                

                resultItem.Data.firmaPaqueteResult = firmaPaqueteResult.Data;
                return Ok(resultItem);
            }
            catch (Exception ex)
            {
                Log.Error("SupplyOrders CompletaFirmaAsync: {error}", ex.Message);
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador";
                return BadRequest(resultItem);
            }
        }

        #endregion

        #region Consulta
        [HttpGet("Consulta/GetOSAsync")]
        [ProducesResponseType(typeof(IEnumerable<SupplyOrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOSAsync(int pageSize, Guid userId, DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1, string search = null, bool esDescarga = false)
        {
            try
            {
                return Ok(await _supplyOrderRepository.GetOSAsync(pageSize, userId, pageNum, fechaInicial, fechaFinal , search, esDescarga));
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