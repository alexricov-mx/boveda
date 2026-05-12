using BERecepcion.Api.Filters;
using BERecepcion.Core.Correos.Dto;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Models;
using BERecepcion.Core.SAPPI.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.Correos
{
    [ApiKeyAuth]
    [Route("api/[controller]")]
    [ApiController]
    public class CorreoController : ControllerBase
    {
        private readonly ICorreoRepository _correoRepository;
        public CorreoController(ICorreoRepository correoRepository)
        {
            _correoRepository = correoRepository;
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> envioCorreoAsync(CorreoDto correoMensaje)
        {
            try
            {
                return Ok(await _correoRepository.EnvioCorreoAsync(correoMensaje));

            }
            catch (Exception ex)
            {
                Log.Error("Correos: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("CorreoArchivos")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> envioCorreoArchivosAsync(DtoPdfFilesEmail dtoPdfFilesEmail)
        {
            try
            {

                return Ok(await _correoRepository.EnvioCorreoArchivosAsync(dtoPdfFilesEmail));

            }
            catch (Exception ex)
            {
                Log.Error("Correos: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("NotificacionFacturaEmailAsync")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> NotificacionFacturaEmailAsync([FromBody] NotificacionFacturaEmailDto dto)
        {
            try
            {
                return Ok(await _correoRepository.NotificacionFacturaEmailAsync(dto.reception, dto.validationError, dto.user, true, "", ""));

            }
            catch (Exception ex)
            {
                Log.Error("Correos: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
    }
}
