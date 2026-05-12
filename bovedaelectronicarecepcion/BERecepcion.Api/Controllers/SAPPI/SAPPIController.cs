using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using BERecepcion.Core.Instrucciones.Interfaces.Repositories;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Dto;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using BERecepcion.Core.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.SAPPI
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class SAPPIController : ControllerBase
    {
        private readonly ISAPPIRepository _sapPIRepository;
        private readonly ISupplyOrderRepository _supplyOrderRepository;
        private readonly ISOEstimationRepository _sOEstimationRepository;
        private readonly ICopadeRepository _copadeRepository;
        private readonly IAnaliticoPagoRepository _analiticoPagoRepository;
        private readonly ICorreoRepository _correoRepository;
        private readonly IUsuariosRepository _usuariosRepository;
        private readonly IBitacoraAdmonRepository _bitacoraAdmonRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly IPaymentScheduleRepository _paymentScheduleRepository;

        public SAPPIController(ISAPPIRepository sAPPIRepository, ICopadeRepository copadeRepository, ICorreoRepository correoRepository, IUsuariosRepository usuariosRepository, ISupplyOrderRepository supplyOrderRepository, ISOEstimationRepository sOEstimationRepository, IAnaliticoPagoRepository analiticoPagoRepository, IBitacoraAdmonRepository bitacoraAdmonRepository, IBitacoraRepository bitacoraRepository, IPaymentScheduleRepository paymentScheduleRepository)
        {
            _sapPIRepository = sAPPIRepository;
            _copadeRepository = copadeRepository;
            _analiticoPagoRepository = analiticoPagoRepository;
            _correoRepository = correoRepository;
            _usuariosRepository = usuariosRepository;
            _supplyOrderRepository = supplyOrderRepository;
            _sOEstimationRepository = sOEstimationRepository;
            _bitacoraAdmonRepository = bitacoraAdmonRepository;
            _bitacoraRepository = bitacoraRepository;
            _paymentScheduleRepository = paymentScheduleRepository;
        }

        [HttpPost("OS_Response")]
        [ProducesResponseType(typeof(DataResult<OSResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> OS_Response([FromBody] OSResponseDto dto)
        {
            // Los metodos de este controller, NO SE USARAN
            try
            {
                return Ok(await _sapPIRepository.PostOS_Response(dto));
            }
            catch (Exception ex)
            {

                Log.Error("OS_Response: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("OSLiberacionVP")]
        [ProducesResponseType(typeof(DataResult<OSLiberacionVPDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> OSLiberacionVP([FromBody] OSLiberacionVPDto dto)
        {
            // Los metodos de este controller, NO SE USARAN
            try
            {
                return Ok(await _sapPIRepository.PostOSLiberacionVP(dto));
            }
            catch (Exception ex)
            {

                Log.Error("OS_Response: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("RE_Response")]
        [ProducesResponseType(typeof(DataResult<REResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RE_Response([FromBody] REResponseDto dto)
        {
            try
            {
                return Ok(await _sapPIRepository.PostRE_Response(dto));
            }
            catch (Exception ex)
            {

                Log.Error("RE_Response: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("NotificaPreFactura")]
        [ProducesResponseType(typeof(DataResult<AnaliticoPagoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> NotificaPreFactura([FromBody] AnaliticoPagoPreFacturaDto dto)
        {
            try
            {
                return Ok(await _sapPIRepository.NotificaPreFactura(dto));
            }
            catch (Exception ex)
            {

                Log.Error("NotificaPreFactura: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("OS_SYNC_IB")]
        [ProducesResponseType(typeof(DataResult<OSResponseItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> OS_SYNC_IB([FromBody] SupplyOrderDto supplyOrderDto)
        {
            try
            {
                DataResult<OSResponseItemDto> response = await _sapPIRepository.PostOS_SYNC_IB(supplyOrderDto);
                if (response.Status == System.Net.HttpStatusCode.OK)
                {
                    // Enviar correo de pendiente de firma

                    DataResult<IEnumerable<UsersDto>> users = new DataResult<IEnumerable<UsersDto>>();
                    DataResult<IEnumerable<UsersDto>> usersRepresentative = new DataResult<IEnumerable<UsersDto>>();
                    string subjectCorreo = "";
                    if (supplyOrderDto.DocumentType.Equals("O"))
                    {
                        users = await _usuariosRepository.GetUsuariosByToken(supplyOrderDto.Signer);
                        subjectCorreo = $"Notificación, Orden de Surtimiento {supplyOrderDto.SAPOrder} pendiente de firma";
                        // Se envia el correo al funcionario la notificacion de firma                         
                        var mail = await _correoRepository.NotificacionOSAsync(users.Data, supplyOrderDto, subjectCorreo);
                        if (mail)
                        {
                            await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                            {
                                Accion = "Envio correo",
                                Descripcion = $"Envio exitoso de correo de Orden Surtimiento {supplyOrderDto.SAPOrder}",
                                Seccion = "OrdenSurtimiento",
                                UserID = users.Data.FirstOrDefault().UserID
                            });
                        }
                        else
                        {
                            await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                            {
                                Accion = "Envio correo",
                                Descripcion = $"Error al enviar correo de Orden Surtimiento {supplyOrderDto.SAPOrder}",
                                Seccion = "OrdenSurtimiento",
                                UserID = users.Data.FirstOrDefault().UserID
                            });
                        }
                        // Obtenemos el GUID del registro en SupplyOrder
                        supplyOrderDto.SupplyOrderID = await _supplyOrderRepository.GetOSSupplyOrderIdAsync(supplyOrderDto);
                        // Se registra el email y la hora del envio
                        await _supplyOrderRepository.SupplyOrderCorreo1Async(supplyOrderDto.SupplyOrderID, users.Data.FirstOrDefault().Email);
                    }
                    else if (supplyOrderDto.DocumentType.Equals("E"))
                    {
                        users = await _usuariosRepository.GetUsuariosByCreditorNumber(supplyOrderDto.CreditorNumber);
                        usersRepresentative = await _usuariosRepository.GetUsuariosByCreditorBanking(supplyOrderDto.Representative);
                        if (users.Data.Count() == 0)
                        {
                            // si no hay un correo para el numero de acreedor, le mandamos el correo al funcionario pemex
                            users = await _usuariosRepository.GetUsuariosByToken(supplyOrderDto.Signer);
                        }
                        subjectCorreo = $"Notificación, Estimación de Obra {supplyOrderDto.SAPOrder} pendiente de firma";

                        SOEstimationDto estimacion = new SOEstimationDto()
                        {
                            Contract = supplyOrderDto.Contract,
                            SapOrder = supplyOrderDto.SAPOrder,
                            CreditorNumber = supplyOrderDto.CreditorNumber,
                            OrganismClave = supplyOrderDto.OrganismClave
                        };
                        try
                        {
                            // Se envia el correo al proveedor la notificacion de firma
                            var mail = await _correoRepository.NotificacionESAsync(users.Data, usersRepresentative.Data, estimacion, subjectCorreo);
                            if (mail)
                            {
                                await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                                {
                                    Accion = "Envio correo",
                                    Descripcion = $"Envio exitoso de correo de Estimación de Obra {estimacion.SapOrder}",
                                    Seccion = "EstimacionObra",
                                    UserID = users.Data.FirstOrDefault().UserID
                                });
                            }
                            else
                            {
                                await _bitacoraRepository.InsertaBitacoraAsync(new BitacoraDto
                                {
                                    Accion = "Envio correo",
                                    Descripcion = $"Error al enviar correo de Estimación de Obra {estimacion.SapOrder}",
                                    Seccion = "EstimacionObra",
                                    UserID = users.Data.FirstOrDefault().UserID
                                });
                            }
                            // Obtenemos el GUID del registro en SOEstimation
                            estimacion.EstimacionID = await _sOEstimationRepository.GetEstimacionIdAsync(estimacion);
                            // Se registra el email y la hora del envio
                            await _sOEstimationRepository.EstimacionCorreo1Async(estimacion.EstimacionID, users.Data.FirstOrDefault().Email);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"OS_SYNC_IB {ex.Message}");
                        }
                    }
                    return Ok(response);
                }
                else
                {
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            catch (Exception ex)
            {
                Log.Error("OS_Response: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("RE_SYNC_IB")]
        [ProducesResponseType(typeof(DataResult<REResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RE_SYNC_IB([FromBody] ReceptionDto ReceptionDto)
        {
            try
            {
                DataResult<REResponseDto> response = await _sapPIRepository.PostRE_SYNC_IB(ReceptionDto);
                if (response.Status == System.Net.HttpStatusCode.OK)
                {
                    DataResult<IEnumerable<UsersDto>> users = new DataResult<IEnumerable<UsersDto>>();
                    users = await _usuariosRepository.GetUsuariosByToken(ReceptionDto.Signer);
                    string subjectCorreo = $"Notificación, Recepción de Almacén {ReceptionDto.Reception} pendiente de firma";
                    try
                    {
                        var correo = await _correoRepository.NotificacionREAsync(users.Data, ReceptionDto, subjectCorreo);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"RE_SYNC_IB {ex.Message}");
                    }
                    // Enviar correo de pendiente de firma
                    return Ok(response);
                }
                else
                {
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            catch (Exception ex)
            {

                Log.Error("RE_Response: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("PP_SYNC_IB")]
        [ProducesResponseType(typeof(DataResult<REResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PP_SYNC_IB([FromBody] PaymentScheduleDto programaPago)
        {
            try
            {
                // Obtenemos los Documentos SAP
                // LlavePP_Sync_OB
                PaymentScheduleDto LlavePPResponse = new PaymentScheduleDto();
                programaPago.vDetalle = LlavePPResponse.vDetalle;

                DataResult<PagoResponseDto> response = await _sapPIRepository.PostPP_SYNC_IB(programaPago);
                if (response.Status == System.Net.HttpStatusCode.OK)
                {
                    DataResult<IEnumerable<UsersDto>> users = new DataResult<IEnumerable<UsersDto>>();
                    users = await _usuariosRepository.GetUsuariosByToken(programaPago.Authorizes);
                    string subjectCorreo = $"Notificación, Programa de Pago {programaPago.ProgramaPago_Id} pendiente de firma";
                    try
                    {
                        var correo = await _correoRepository.NotificacionPPAsync(users.Data, programaPago, subjectCorreo);
                        // actualizar el campo con la fecha del envio del correo al usuario que Autoriza
                        await _paymentScheduleRepository.PaymentMailAsync(response.Data.ID, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"PP_SYNC_IB {ex.Message}");
                    }
                    // Enviar correo de pendiente de firma
                    return Ok(response);
                }
                else
                {
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            catch (Exception ex)
            {

                Log.Error("RE_Response: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("LP_SYNC_IB")]
        [ProducesResponseType(typeof(DataResult<REResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LP_SYNC_IB([FromBody] PaymentListDto listaPago)
        {
            try
            {
                DataResult<PagoResponseDto> response = await _sapPIRepository.PostLP_SYNC_IB(listaPago);
                if (response.Status == System.Net.HttpStatusCode.OK)
                {
                    DataResult<IEnumerable<UsersDto>> users = new DataResult<IEnumerable<UsersDto>>();
                    users = await _usuariosRepository.GetUsuariosByToken(listaPago.Authorizes);
                    string subjectCorreo = $"Notificación, Lista de Pago {listaPago.ListaPago_Id} pendiente de firma";
                    try
                    {
                        var correo = await _correoRepository.NotificacionLPAsync(users.Data, listaPago, subjectCorreo);
                        // actualizar el campo con la fecha del envio del correo
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"LP_SYNC_IB {ex.Message}");
                    }
                    // Enviar correo de pendiente de firma
                    return Ok(response);
                }
                else
                {
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            catch (Exception ex)
            {

                Log.Error("RE_Response: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("AP_SYNC_IB")]
        [ProducesResponseType(typeof(DataResult<AnaliticoPagoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AP_SYNC_IB([FromBody] AnaliticoPagoDto apDto)
        {
            try
            {
                DataResult<AnaliticoPagoResponseDto> response = await _sapPIRepository.PostAP_SYNC_IB(apDto);
                if (response.Status == System.Net.HttpStatusCode.OK)
                {
                    var analiticoPago = await _analiticoPagoRepository.GetAPByIdAnaliticoAsync(apDto.IdAnalitico);
                    var users = await _analiticoPagoRepository.GetEmailAP(analiticoPago.Data.AnaliticoPagoID);
                    string subjectCorreo = $"Notificación, Analitico de Pago {apDto.IdAnalitico} pendiente de firma";
                    try
                    {
                        var correo = await _correoRepository.NotificacionAPAsync(users.Data, analiticoPago.Data, subjectCorreo);

                        // Registramos el envio del correo                        
                        await _analiticoPagoRepository.APFirmaCorreoAsync(analiticoPago.Data.AnaliticoPagoID, users.Data);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"AP_SYNC_IB {ex.Message}");
                    }
                    // Enviar correo de pendiente de firma
                    return Ok(response);
                }
                else
                {
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            catch (Exception ex)
            {

                Log.Error("RE_Response: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("CopadeCorp")]
        [ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaCopadeCorp([FromBody] CopadeDto copadeReq)
        {
            try
            {
                // hay que mandar el correo de notificacion de firma al firmante 1
                // (corporativo solo tiene 1 firmante)                
                DataResult<string> response = await _sapPIRepository.InsertaCopadeAsync(copadeReq);

                if (response.Status == System.Net.HttpStatusCode.OK)
                {
                    // Enviar correo de pendiente de firma
                    PICopadeRequestDto copadeReqGUID = new PICopadeRequestDto()
                    {
                        Clave = copadeReq.clave,
                        SapOrder = copadeReq.SapOrder,
                        Reception = copadeReq.Reception,
                        Exercise = copadeReq.Exercise
                    };
                    Guid copadeID = await _sapPIRepository.GetCopadeIdAsync(copadeReqGUID);
                    var usersCopade = await _copadeRepository.GetSigners2ByCopadeID(copadeID);
                    var firmante1 = await _usuariosRepository.GetUsuarioEmailAsync(usersCopade.Data.signer1, usersCopade.Data.signer1Email);
                    var listaCorreos = !string.IsNullOrWhiteSpace(firmante1.Data.Email) ? firmante1.Data.Email : "";
                    CopadeDto copadeCorreo = new CopadeDto()
                    {
                        Contract = copadeReq.Contract,
                        CreditorNumber = copadeReq.CreditorNumber,
                        CreditorRfc = copadeReq.CreditorRfc,
                        Creditor = copadeReq.Creditor,
                        Reception = copadeReq.Reception
                    };

                    try
                    {
                        List<UsersDto> firmantes1List = new List<UsersDto>();
                        if (firmante1.Data != null)
                            firmantes1List.Add(firmante1.Data);

                        DataResult<IEnumerable<UsersDto>> firmanteCorpo = new DataResult<IEnumerable<UsersDto>>()
                        {
                            Data = firmantes1List
                        };
                        var correo = await _correoRepository.NotificacionCOPADEAsync(firmanteCorpo.Data, copadeCorreo, $"Notificación, COPADE {copadeReq.Reception} pendiente de firma");
                        var bitacoraMensaje = "";
                        if (correo)
                        {
                            foreach (var item in firmantes1List)
                            {
                                bitacoraMensaje = $"Notificación, COPADE {copadeReq.Reception} pendiente de firma";
                                var bitacora = new BitacoraAdmonDto
                                {
                                    Evento = "Correo Copade",
                                    Usuario = item.Name,
                                    Descripcion = bitacoraMensaje
                                };
                                await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                            }
                        }
                        else
                        {
                            foreach (var item in firmantes1List)
                            {
                                bitacoraMensaje = $"Notificación, COPADE {copadeReq.Reception} pendiente de firma - Problema con envio de correo {item.Email}";
                                var bitacora = new BitacoraAdmonDto
                                {
                                    Evento = "Correo Copade",
                                    Usuario = item.Name,
                                    Descripcion = bitacoraMensaje
                                };
                                await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                            }
                        }
                        // Registramos el envio del correo                        
                        await _copadeRepository.CopadeFirmaCorreoAsync(copadeID, listaCorreos, "1");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"CopadeCorp {ex.Message}");
                    }

                    //await _copadeRepository.CopadeFirmaCorreoAsync(copadeID, dto.User.UserType, dto.User.Email)
                    return Ok(response);
                }
                else
                {
                    return Problem(null, null, 500, "Error interno", null);
                }

            }
            catch (Exception ex)
            {

                Log.Error("CopadeCorp: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("LlaveCopade")]
        [ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LlaveCopade([FromBody] PICopadeRequestDto copadeReq)
        {
            try
            {
                // recibimos el Request
                // y llamamos a la BAPI para traer el Copade
                var recuperaCopadeResult = await _sapPIRepository.RecuperaCopadeAsync(copadeReq);
                if (recuperaCopadeResult.Status == System.Net.HttpStatusCode.OK)
                {
                    DataResult<string> response = await _sapPIRepository.InsertaLlaveCopadeAsync(recuperaCopadeResult.Data);

                    // hay que mandar el correo de notificacion de firma al firmante 1
                    // (PEP solo tiene 1 firmante y PTRI tiene 2 firmantes)
                    if (string.IsNullOrEmpty(copadeReq.Exercise))
                        copadeReq.Exercise = recuperaCopadeResult.Data.Exercise;

                    Guid copadeID = await _sapPIRepository.GetCopadeIdAsync(copadeReq);
                    var usersCopade = await _copadeRepository.GetSigners2ByCopadeID(copadeID);

                    var firmante1 = await _usuariosRepository.GetUsuarioEmailAsync(usersCopade.Data.signer1, usersCopade.Data.signer1Email);
                    var suplente1 = await _usuariosRepository.GetUsuarioEmailAsync(usersCopade.Data.alternate1, usersCopade.Data.alternate1Email);

                    List<UsersDto> firmantes1List = new List<UsersDto>();
                    if (firmante1.Data.Email != null)
                        firmantes1List.Add(firmante1.Data);

                    if (suplente1.Data.Email != null)
                        firmantes1List.Add(suplente1.Data);

                    var listaCorreos = !string.IsNullOrWhiteSpace(firmante1.Data.Email) ? firmante1.Data.Email : "";
                    listaCorreos += !string.IsNullOrWhiteSpace(suplente1.Data.Email) ? $",{suplente1.Data.Email}" : "";

                    CopadeDto copadeCorreo = new CopadeDto()
                    {
                        Contract = recuperaCopadeResult.Data.Contract,
                        CreditorNumber = recuperaCopadeResult.Data.CreditorNumber,
                        CreditorRfc = recuperaCopadeResult.Data.CreditorRfc,
                        Creditor = recuperaCopadeResult.Data.Creditor,
                        Reception = recuperaCopadeResult.Data.Reception
                    };

                    try
                    {
                        var correo = await _correoRepository.NotificacionCOPADEAsync(firmantes1List, copadeCorreo, $"Notificación, COPADE {copadeReq.Reception} pendiente de firma");
                        var bitacoraMensaje = "";
                        if (correo)
                        {
                            foreach (var item in firmantes1List)
                            {
                                bitacoraMensaje = $"Notificación, COPADE {copadeReq.Reception} pendiente de firma";
                                var bitacora = new BitacoraAdmonDto
                                {
                                    Evento = "Correo Copade",
                                    Usuario = item.Name,
                                    Descripcion = bitacoraMensaje
                                };
                                await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                            }
                        }
                        else
                        {
                            foreach (var item in firmantes1List)
                            {
                                bitacoraMensaje = $"Notificación, COPADE {copadeReq.Reception} pendiente de firma - Problema con envio de correo {item.Email}";
                                var bitacora = new BitacoraAdmonDto
                                {
                                    Evento = "Correo Copade",
                                    Usuario = item.Name,
                                    Descripcion = bitacoraMensaje
                                };
                                await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);
                            }
                        }
                        // Registramos el envio del correo                        
                        await _copadeRepository.CopadeFirmaCorreoAsync(copadeID, listaCorreos, "1");

                        // hay que hacer una bitacora de correos
                    }
                    catch (Exception ex)
                    {

                        Log.Error($"LlaveCopade {ex.Message}");
                    }

                    return Ok(response);
                }
                else
                {
                    Log.Error("Llave copade");
                    return Problem(null, null, 500, "Error interno", null);
                }
            }
            catch (Exception ex)
            {

                Log.Error("OS_Response: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        // recupera el copade
        [HttpPost("RecuperaCopade")]
        [ProducesResponseType(typeof(DataResult<CopadeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecuperaCopade([FromBody] PICopadeRequestDto copadeReq)
        {
            // Organismo (clave) = PCORP, PEP, PTRI
            // SapOrder
            // Reception
            // Exercise
            try
            {
                DataResult<CopadeDto> CopadeResponse = new DataResult<CopadeDto>();
                if (copadeReq.Clave.Equals("PCORP"))
                {
                    CopadeResponse = await _sapPIRepository.RecuperaCopadeBDAsync(copadeReq);
                }
                else // PEP o PTRI
                {
                    CopadeResponse = await _sapPIRepository.RecuperaCopadeAsync(copadeReq);
                    copadeReq.Exercise = CopadeResponse.Data.Exercise;
                    if (CopadeResponse.Data != null)
                    {
                        CopadeResponse.Data.CopadeID = await _sapPIRepository.GetCopadeIdAsync(copadeReq);

                        // para guardar el copade en la BD                                               
                        var tmp = await _sapPIRepository.InsertaCopadeAsync(CopadeResponse.Data);
                        CopadeResponse.Data.LogoB64 = await _sapPIRepository.GetLogo(copadeReq.Clave);
                    }
                }
                return Ok(CopadeResponse);
            }
            catch (Exception ex)
            {
                Log.Error("RecuperaCopade: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        // recupera PreFacturaXML
        [HttpPost("RecuperaPreFacturaXML/{copadeId}")]
        [ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecuperaPreFacturaXML(Guid copadeId)
        {
            try
            {
                var tmp = await _sapPIRepository.GetPreFacturaById(copadeId);
                byte[] bytes = Encoding.Default.GetBytes(tmp.Data);
                tmp.Data = Encoding.UTF8.GetString(bytes);
                return Ok(tmp);
            }
            catch (Exception ex)
            {

                Log.Error("RecuperaPreFacturaXML: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        // recupera NotaCreditoXML
        [HttpPost("RecuperaNotaCreditoXML/{copadeId}")]
        [ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecuperaNotaCreditoXML(Guid copadeId)
        {
            try
            {
                var tmp = await _sapPIRepository.GetNotaCreditoById(copadeId);
                byte[] bytes = Encoding.Default.GetBytes(tmp.Data);
                tmp.Data = Encoding.UTF8.GetString(bytes);
                return Ok(tmp);
            }
            catch (Exception ex)
            {

                Log.Error("RecuperaNotaCreditoXML: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("ValidacionMiro")]
        [ProducesResponseType(typeof(DataResult<ValidacionMiroDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidacionMiro([FromBody] ValidacionMiroDto miroDto)
        {
            try
            {
                ValidacionCXPDto dto = new ValidacionCXPDto() { Contrato = miroDto.Contrato, Fecha = miroDto.Fecha, Organismo = miroDto.Organismo };
                var r = await _sapPIRepository.PostValidacionCxP(dto);

                return Ok(new ValidacionMiroDto() { Mensaje = r.Data.Mensaje, Status = r.Data.Status });
            }
            catch (Exception ex)
            {

                Log.Error("ValidacionMiro: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("CxP")]
        [ProducesResponseType(typeof(DataResult<CXPDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CxP([FromBody] DataResult<CXPDto> dto)
        {
            try
            {
                return Ok(await _sapPIRepository.PostCxP(dto.Data));
            }
            catch (Exception ex)
            {

                Log.Error("CxP: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
    }
}
