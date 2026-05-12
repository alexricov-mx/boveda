using BERecepcion.Api.Filters;
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
    public class RecepcionElectronicaPController : ControllerBase
    {

        private readonly IRecepcionElectronicaPRepository _recepcionElectronicaPRepository;

        public RecepcionElectronicaPController(IRecepcionElectronicaPRepository ecepcionElectronicaPRepository)
        {
            _recepcionElectronicaPRepository = ecepcionElectronicaPRepository;
           
        }


        [HttpPost("SaveInvoiceREP")]
        [ProducesResponseType(typeof(DataResult<RecepcionElectronicaPDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveInvoiceREP([FromBody] DataResult<RecepcionElectronicaPDto> dto)
        {
            try
            {
                var result = await _recepcionElectronicaPRepository.SaveInvoiceREP(dto.Data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("RecepcionElectronicaPController: SaveInvoiceREP {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("SavePagoREP")]
        [ProducesResponseType(typeof(DataResult<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SavePagoREP([FromBody] DataResult<RecepcionElectronicaPDto> dto)
        {
            try
            {
                var result = await _recepcionElectronicaPRepository.SavePagoREP(dto.Data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("RecepcionElectronicaPController: SavePagoREP {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

    }
}
