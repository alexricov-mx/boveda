using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Cancelaciones.Dto;
using BERecepcion.Core.Cancelaciones.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.Cancelaciones
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class CancelacionesController : ControllerBase
    {


        private readonly ICancelacionesRepository _cancelacionesRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        public CancelacionesController(ICancelacionesRepository preFacturaRepository, IBitacoraRepository bitacoraRepository)
        {
            _cancelacionesRepository = preFacturaRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        #region CancelacionCopade
        [HttpGet("GetBusquedaCancelacionesCopadeAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<CancelacionesCopadeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBusquedaCancelacionesCopadeAsync(string token, Guid userId, int pageSize, string busqueda = null, DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1)
        {
            try
            {
                return Ok(await _cancelacionesRepository.GetBusquedaCancelacionesCopadeAsync(token, userId, fechaInicial, fechaFinal, busqueda, pageSize, pageNum));
            }
            catch (Exception ex)
            {
                Log.Error("GetBusquedaCancelacionesCopadeAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }


        [HttpPost("CancelacionesCopadeMasivaAsync")]
        [ProducesResponseType(typeof(DataResult<CancelacionesCopadeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelacionesCopadeMasivaAsync([FromBody] DataResult<CancelacionesCopadeDto> dto)
        {
            try
            {

                var result = await _cancelacionesRepository.CancelacionesCopadeMasiva(dto.Data);
                if (result.Status == System.Net.HttpStatusCode.OK)
                {
                    foreach (var item in dto.Data.listCopades)
                    {
                        if (item.Status)
                        {
                            var bitacora = new BitacoraDto
                            {
                                UserID = dto.User.UserID,
                                Seccion = "Copade",
                                //Accion = String.Format("Cancela copades Clave: {0} Organismo: {1}",dto.Data.clave) ,
                                Accion = "Cancela",
                                Descripcion = $"Se cancela el copade {item.Reception} Organismo: {item.clave}"
                            };

                            await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
                        }
                    }
                }

                result.Message = result.Message;

                return Ok(result);

            }
            catch (Exception ex)
            {
                Log.Error("CancelacionesCopadeMasiva: Cancela Copade {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        #endregion CancelacionCopade

        #region PaymentSchedule

        [HttpGet("GetBusquedaCancelacionesPaymentSchedule")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<PaymentScheduleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBusquedaCancelacionesPaymentScheduleAsync(string token, Guid userId, int pageSize, string busqueda = null, DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1)
        {
            try
            {
                return Ok(await _cancelacionesRepository.GetBusquedaCancelacionesPaymentScheduleAsync(token, userId, fechaInicial, fechaFinal, busqueda, pageSize, pageNum));
            }
            catch (Exception ex)
            {
                Log.Error("GetBusquedaCancelacionesPaymentScheduleAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }


        [HttpPost("CancelacionesPaymentScheduleMasivaAsync")]
        [ProducesResponseType(typeof(DataResult<PaymentScheduleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelacionesPaymentScheduleMasivaAsync([FromBody] DataResult<PaymentScheduleDto> dto)
        {
            try
            {

                var result = await _cancelacionesRepository.CancelacionesPaymentScheduleMasivaAsync(dto.Data);


                if (result.Status == System.Net.HttpStatusCode.OK)
                {

                    foreach (var item in dto.Data.CPP)
                    {
                        if (item.Status)
                        {
                            var bitacora = new BitacoraDto
                            {
                                UserID = dto.User.UserID,
                                Seccion = "Programa de Pagos",
                                Accion = "Cancela",
                                Descripcion = $"Se cancela el Programa de Pago {item.ProgramaPago_Id}"
                            };

                            await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
                        }
                    }
                }

                result.Message = result.Message;

                return Ok(result);

            }
            catch (Exception ex)
            {
                Log.Error("CancelacionesPaymentScheduleeMasivaAsync: Cancela PaymentSchedule {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        #endregion PaymentSchedule

        #region PaymentList

        [HttpGet("GetBusquedaCancelacionesPaymentListAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<PaymentListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBusquedaCancelacionesPaymentListAsync(string token, Guid userId, int pageSize, string busqueda = null, DateTime? fechaInicial = null, DateTime? fechaFinal = null, int pageNum = 1)
        {
            try
            {
                return Ok(await _cancelacionesRepository.GetBusquedaCancelacionesPaymentListAsync(token, userId, fechaInicial, fechaFinal, busqueda, pageSize, pageNum));
            }
            catch (Exception ex)
            {
                Log.Error("GetBusquedaCancelacionesPaymentListAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }


        [HttpPost("CancelacionesPaymentListMasivaAsync")]
        [ProducesResponseType(typeof(DataResult<PaymentListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelacionesPaymentListMasivaAsync([FromBody] DataResult<PaymentListDto> dto)
        {
            try
            {

                var result = await _cancelacionesRepository.CancelacionesPaymentListMasivaAsync(dto.Data);


                if (result.Status == System.Net.HttpStatusCode.OK)
                {
                    foreach (var item in dto.Data.LPP)
                    {
                        if (item.Status)
                        {
                            var bitacora = new BitacoraDto
                            {
                                UserID = dto.User.UserID,
                                Seccion = "ListaPago",
                                Accion = "Cancela",
                                Descripcion = $"Se cancela la Lista de Pago {item.ListaPago_Id}"
                            };

                            await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
                        }
                    }
                }

                result.Message = result.Message;

                return Ok(result);

            }
            catch (Exception ex)
            {
                Log.Error("CancelacionesPaymentScheduleeMasivaAsync: Cancela PaymentSchedule {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        #endregion PaymentList

    }
}
