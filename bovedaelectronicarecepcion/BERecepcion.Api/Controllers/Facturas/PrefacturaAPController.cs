using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.Facturas
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class PrefacturaAPController : ControllerBase
    {
        private readonly IPrefacturaAPRepository _preFacturaAPRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        public PrefacturaAPController(IPrefacturaAPRepository preFacturaAPRepository, IBitacoraRepository bitacoraRepository)
        {
            _preFacturaAPRepository = preFacturaAPRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        [HttpGet("GetPreFacturaAPAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<PrefacturaAPDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPreFacturaAPAsync(string start, string end, string search, string creditorNumber, Guid userId, int pageSize, int pageNum = 1)
        {
            try
            {
                return Ok(await _preFacturaAPRepository.GetPreFacturaAsync(start, end, search, creditorNumber, userId, pageSize, pageNum));
            }
            catch (Exception ex)
            {
                Log.Error("GetPreFacturaAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetPreFacturaXmlAPAsync")]
        [ProducesResponseType(typeof(DataResult<PrefacturaAPDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPreFacturaXmlAPAsync(Guid AnaliticoPagoID)
        {
            try
            {
                return Ok(await _preFacturaAPRepository.GetPreFacturaXmlAPAsync(AnaliticoPagoID));
            }
            catch (Exception ex)
            {
                Log.Error("GetPreFacturaAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }


        [HttpPost("GetPreFacturaXmlAPMasAsync")]
        [ProducesResponseType(typeof(DataResult<PrefacturaAPDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPreFacturaXmlAPMasAsync([FromBody] DataResult<IEnumerable<PrefacturaAPDto>> dto)
        {
            try
            {
                return Ok(await _preFacturaAPRepository.GetPreFacturaXmlAPMasAsync(dto.Data));
            }
            catch (Exception ex)
            {
                Log.Error("GetPreFacturaAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }


    }
}
