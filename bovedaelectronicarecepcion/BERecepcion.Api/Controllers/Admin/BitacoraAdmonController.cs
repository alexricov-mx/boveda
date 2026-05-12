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
    public class BitacoraAdmonController : ControllerBase
    {
        private readonly IBitacoraAdmonRepository _bitacoraAdmonRepository;
        public BitacoraAdmonController(IBitacoraAdmonRepository bitacoraAdmonRepository)
        {
            _bitacoraAdmonRepository = bitacoraAdmonRepository;
        }
        [HttpGet("GetBitacoraAdmonAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<BitacoraAdmonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBitacoraAdmonAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda, int pageSize, int pageNum = 1)
        {
            var result = await _bitacoraAdmonRepository.GetBitacoraAdmonAsync(fechaInicial, fechaFinal, busqueda, pageSize, pageNum);
            return result.ToActionResult();
        }
        [HttpGet("GetAllBitacoraAdmonAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<BitacoraAdmonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBitacoraAdmonAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda)
        {
            var result = await _bitacoraAdmonRepository.GetAllBitacoraAdmonAsync(fechaInicial, fechaFinal, busqueda);
            return result.ToActionResult();
        }

        [HttpPost("InsertaBitacoraAdmonAsync")]
        [ProducesResponseType(typeof(DataResult<BitacoraAdmonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaBitacoraAdmonAsync(DataResult<BitacoraAdmonDto> dto)
        {
            var result = await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(dto.Data);
            return result.ToActionResult();
        }
    }
}
