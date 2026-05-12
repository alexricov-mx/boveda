using BERecepcion.Api.Filters;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Interfaces.Repositories;
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
    public class RecepcionEPController : ControllerBase
    {

        private readonly IRecepcionEPRepository _recepcionEPRepository;

        public RecepcionEPController(IRecepcionEPRepository recepcionEPRepository)
        {
            _recepcionEPRepository = recepcionEPRepository;
        }

        [HttpGet("GetValidaInvoiceDocAsync/{uuid}")]
        [ProducesResponseType(typeof(DataResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetValidaInvoiceDocAsync(Guid uuid)
        {
            try
            {
                return Ok(await _recepcionEPRepository.GetValidaInvoiceDocAsync(uuid));
            }
            catch (Exception ex)
            {
                Log.Error("GetValidaInvoiceDocAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("InsertaPagosDocAsync")]
        [ProducesResponseType(typeof(DataResult<RecepcionElectronicaPDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaPagosDocAsync([FromBody] DataResult<RecepcionElectronicaPDto> dto)
        {
            try
            {
                
                var result = await _recepcionEPRepository.InsertaPagosDocAsync(dto.Data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: SaveInvoice {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
    }
}
