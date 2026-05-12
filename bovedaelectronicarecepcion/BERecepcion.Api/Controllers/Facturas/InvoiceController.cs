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
    public class InvoiceController : ControllerBase
    {
        protected IInvoiceRepository _invoiceRepository;
        public InvoiceController(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }
        #region Copade
        [HttpGet("GetInvoiceByReception")]
        [ProducesResponseType(typeof(DataResult<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceByReception(string clave, string reception, string exercise)
        {
            try
            {
                return Ok(await _invoiceRepository.GetInvoiceByReception(clave, reception, exercise));
            }
            catch (Exception ex)
            {
                Log.Error("InvoiceController: GetInvoiceByReception {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
        
        #endregion
        #region AP
        [HttpGet("GetInvoiceAPByReception/{idAnalitico}")]
        [ProducesResponseType(typeof(DataResult<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceAPByReception(string idAnalitico)
        {
            try
            {
                return Ok(await _invoiceRepository.GetInvoiceByAnaliticoPago(idAnalitico));
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: GetInvoiceByReception {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
        #endregion
    }
}
