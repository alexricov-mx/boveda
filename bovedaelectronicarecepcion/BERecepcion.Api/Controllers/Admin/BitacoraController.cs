using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class BitacoraController : ControllerBase
    {
        private readonly IBitacoraRepository _bitacoraRepository;
        public BitacoraController(IBitacoraRepository bitacoraRepository)
        {
            _bitacoraRepository = bitacoraRepository;
        }
        [HttpGet("GetBitacoraAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<BitacoraDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBitacoraAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda, int pageSize, int pageNum = 1)
        {
            var result = await _bitacoraRepository.GetBitacoraAsync(fechaInicial, fechaFinal, busqueda, pageSize, pageNum);
            return result.ToActionResult();
        }
        [HttpGet("GetAllBitacoraAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<BitacoraDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBitacoraAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda)
        {
            var result = await _bitacoraRepository.GetAllBitacoraAsync(fechaInicial, fechaFinal, busqueda);
            return result.ToActionResult();
        }
        [HttpPost("InsertaBitacoraAsync")]
        [ProducesResponseType(typeof(DataResult<BitacoraDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaBitacoraAsync(DataResult<BitacoraDto> dto)
        {
            var result = await _bitacoraRepository.InsertaBitacoraAsync(dto.Data);
            return result.ToActionResult();
        }

        [HttpPost("InsertaBitacoraInvoiceAsync")]
        [ProducesResponseType(typeof(DataResult<BitacoraInvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaBitacoraInvoiceAsync(DataResult<BitacoraInvoiceDto> dto)
        {
            var result = await _bitacoraRepository.InsertaBitacoraInvoiceAsync(dto.Data);
            return result.ToActionResult();
        }

        [HttpPost("InsertaBitacoraInvoiceErrorAsync")]
        [ProducesResponseType(typeof(DataResult<BitacoraInvoiceErrorInsertDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaBitacoraInvoiceErrorAsync(DataResult<BitacoraInvoiceErrorInsertDto> dto)
        {
            var result = await _bitacoraRepository.InsertaBitacoraInvoiceErrorAsync(dto.Data);
            return result.ToActionResult();
        }
    }
}
