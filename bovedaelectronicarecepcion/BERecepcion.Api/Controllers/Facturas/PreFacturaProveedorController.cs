using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
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
    public class PreFacturaProveedorController : ControllerBase
    {

        private readonly IPreFacturaRepository _preFacturaRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        public PreFacturaProveedorController(IPreFacturaRepository preFacturaRepository, IBitacoraRepository bitacoraRepository)
        {
            _preFacturaRepository = preFacturaRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(DataResult<IEnumerable<PreFacturaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPreFacturaAsync(string start, string end, string search, string creditorNumber, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            try
            {
                return Ok(await _preFacturaRepository.GetPreFacturaAsync(start, end, search, creditorNumber, userId, pageSize, pageNum, esDescarga));
            }
            catch (Exception ex)
            {
                Log.Error("GetPreFacturaAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetPreFacturaXmlAsync")]
        [ProducesResponseType(typeof(DataResult<PreFacturaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPreFacturaXmlAsync(Guid CopadeID)
        {
            try
            {
                return Ok(await _preFacturaRepository.GetPreFacturaXmlAsync(CopadeID));
            }
            catch (Exception ex)
            {
                Log.Error("GetPreFacturaAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }


        [HttpPost("GetPreFacturaXmlMasAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<PreXmlMasDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPreFacturaXmlMasAsync([FromBody] DataResult<IEnumerable<PreXmlMasDto>> dto)
        {
            try
            {
                return Ok(await _preFacturaRepository.GetPreFacturaXmlMasAsync(dto.Data));
            }
            catch (Exception ex)
            {
                Log.Error("GetPreFacturaAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetPreFacturaBuscarAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<PreFacturaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPreFacturaBuscarAsync(string creditorNumber, int pageSize, int pageNum = 1, string busqueda = null, DateTime? fechaInicial = null, DateTime? fechaFinal = null)
        {
            try
            {
                return Ok(await _preFacturaRepository.GetPreFacturaBuscarAsync(creditorNumber, pageSize, pageNum, busqueda, fechaInicial, fechaFinal));
            }
            catch (Exception ex)
            {
                Log.Error("GetPreFacturaAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

    }
}
