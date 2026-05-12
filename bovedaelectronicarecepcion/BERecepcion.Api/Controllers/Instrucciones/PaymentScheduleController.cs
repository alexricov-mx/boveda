using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.Instrucciones.Dto;
using BERecepcion.Core.Instrucciones.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BERecepcion.Api.ModelBinding;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Dto;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using BERecepcion.Core.IntegracionEFirma;
using BERecepcion.Infraestructura.Instrucciones.Repositories;


namespace BERecepcion.Api.Controllers.Instrucciones
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class PaymentScheduleController : ControllerBase
    {
        private readonly IPaymentListRepository _paymentListRepository;
        private readonly IPaymentScheduleRepository _paymentScheduleRepository;
        private readonly IConfiguration _configuration;
        private readonly IDocumentoFirmadoRepository _documentoFirmadoRepository;
        private readonly IESignRepository _eSignRepository;
        private readonly IHostEnvironment _env;
        private readonly IDocumentosRepository _documentosRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly IUsuariosRepository _usuariosRepository;
        private readonly ICorreoRepository _correoRepository;

        public PaymentScheduleController(IPaymentScheduleRepository paymentScheduleRepository, IConfiguration configuration, IDocumentoFirmadoRepository documentoFirmadoRepository, IESignRepository eSignRepository, IHostEnvironment env, IDocumentosRepository documentosRepository, IBitacoraRepository bitacoraRepository, IUsuariosRepository usuariosRepository, ICorreoRepository correoRepository)
        {
            _paymentScheduleRepository = paymentScheduleRepository;
            _configuration = configuration;
            _documentoFirmadoRepository = documentoFirmadoRepository;
            _eSignRepository = eSignRepository;
            _env = env;
            _documentosRepository = documentosRepository;
            _bitacoraRepository = bitacoraRepository;
            _usuariosRepository = usuariosRepository;
            _correoRepository = correoRepository;
        }
        [HttpGet("GetPayments")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<PaymentScheduleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPayments(string Token, int pageSize, string search, int pageNum = 1)
        {
            try
            {
                return Ok(await _paymentScheduleRepository.GetPayments(Token, pageSize, search, pageNum));
            }
            catch (Exception ex)
            {
                Log.Error("GetPayments: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        #region Firma programas de pago
        [HttpPost("FirmarAsync")]
        [ProducesResponseType(typeof(DataResult<ExternosDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FirmarAsync([FromBody] DataResult<Externos2Dto> model)
        {
            DataResult<Externos2Dto> resultItem = new DataResult<Externos2Dto>
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Envío Exitoso FirmarAsync",
                Data = new Externos2Dto()
            };
          
            try
            {
                string NombreArchivo = string.Concat("Documento_", model.Data.paquete.Titulo, ".pdf");

                DataResult<ArchivoPDFDto> archivoPDF = new DataResult<ArchivoPDFDto>();
                string archivo = "";
                IFormFile documentoPDF;
                byte[] bytes;
                string basePath = _env.ContentRootPath + "/Files/";
                bool basePathExists = Directory.Exists(basePath);
                if (!basePathExists) Directory.CreateDirectory(basePath);

                var _archivoPDF = await _documentosRepository.GetDocumentoListaPagoAsync(model.Data.paymentList.ListaPago_Id);
                if (archivoPDF.Status != System.Net.HttpStatusCode.OK)
                {
                    archivo = basePath + "prueba.pdf";
                }
                else
                {
                    bytes = Convert.FromBase64String(archivoPDF.Data.ARCHIVO);
                    archivo = basePath + model.Data.paymentList.ListaPago_Id + ".pdf";
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
                Log.Error("PaymentSchedule FirmarAsync: {error}", ex.Message);
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
                DataResult<FirmarPaqueteResult> firmaPaqueteResult = await _eSignRepository.PostFirmaEFirmaAsync(externos.Data.firmaPaquete);
                if (firmaPaqueteResult.Status != System.Net.HttpStatusCode.OK)
                {
                    Log.Error("Externos PostDocumentoAsync eSignFirma: {error}", firmaPaqueteResult.Message);
                    resultItem.Message = firmaPaqueteResult.Message;
                    resultItem.Status = firmaPaqueteResult.Status;
                    resultItem.Data.firmaPaqueteResult = firmaPaqueteResult.Data;
                    return resultItem;
                }

                await _paymentListRepository.PaymentSignAsync(externos.Data.paymentList.PaymentListID);
                // escribimos en bitacora
                await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto()
                {
                    UserID = externos.Data.usuarioBEId,
                    Seccion = "Programación de Pago",
                    Accion = "Firmar",
                    Descripcion = $"Firma de programación de pago {externos.Data.paymentList.ListaPago_Id}"
                });

                var users = await _usuariosRepository.GetUsuariosByToken(externos.Data.paymentList.TokenTo);
                var mail = await _correoRepository.NotificacionLPAsync(users.Data, externos.Data.paymentList, "Firma de Programa de pago");
                firmaPaqueteResult.Status = mail ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Conflict;
                // registramos evento de correo

                await _paymentListRepository.PaymentMailAsync(externos.Data.paymentList.PaymentListID);
                return resultItem;
            }
            catch (Exception ex)
            {
                Log.Error("SupplyOrders CompletaFirmaAsync: {error}", ex.Message);
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador";
                return resultItem;
            }
        }
        #endregion

    }
}