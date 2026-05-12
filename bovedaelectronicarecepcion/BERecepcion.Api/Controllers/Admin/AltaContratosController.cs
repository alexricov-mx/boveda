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
    public class AltaContratosController : ControllerBase
    {
        private readonly IAltaContratosRepository _altaContratosRepository;
        private readonly IBitacoraRepository _bitacoraRepository;

        public AltaContratosController(IAltaContratosRepository altaContratosRepository, IBitacoraRepository bitacoraRepository)
        {
            _altaContratosRepository = altaContratosRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        [HttpGet("ConsultaACAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<AltaContratosDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetACAsync(int pageSize, int pageNum = 1, string search = null, bool esDescarga = false)
        {
            var result = await _altaContratosRepository.ConsultaACAsync(pageSize, pageNum, search, esDescarga);
            return result.ToActionResult();
        }
        [HttpGet("GetACByIdAsync")]
        [ProducesResponseType(typeof(DataResult<AltaContratosDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetACByIdAsync(Guid AltaContratoID)
        {
            var result = await _altaContratosRepository.GetACByIdAsync(AltaContratoID);
            return result.ToActionResult();
        }
        [HttpGet("ConsultaACGetAllAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<AltaContratosDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultaACGetAllAsync(int pageSize, int pageNum = 1)
        {
            var result = await _altaContratosRepository.ConsultaACGetAllAsync(pageSize, pageNum);
            return result.ToActionResult();
        }

        [HttpPost("InsertaACAsync")]
        [ProducesResponseType(typeof(DataResult<AltaContratosDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertACAsync([FromBody] DataResult<AltaContratosDto> altaContratosDto)
        {
            var result = await _altaContratosRepository.InsertaACAsync(altaContratosDto.Data);
            if (result.Status == HttpStatusCode.InternalServerError)
                throw new Exception(result.Message);
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = altaContratosDto.User.UserID,
                    Seccion = "Contratos",
                    Accion = "Alta",
                    Descripcion = string.Concat("Se da de alta el contrato " + altaContratosDto.Data.Contract)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            return result.ToActionResult();
        }

        [HttpPost("BorraACAsync")]
        [ProducesResponseType(typeof(DataResult<AltaContratosDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteACAsync([FromBody] DataResult<AltaContratosDto> ACID)
        {
            var result = await _altaContratosRepository.BorraACAsync(ACID.Data.AltaContratoID);
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = ACID.User.UserID,
                    Seccion = "Contratos",
                    Accion = "Eliminar",
                    Descripcion = string.Concat("Se elimina el contrato " + result.Data.Contract)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            return result.ToActionResult();
        }

        [HttpPost("ActualizaACAsync")]
        [ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaACAsync([FromBody] DataResult<AltaContratosDto> ACDto)
        {
            var result = await _altaContratosRepository.ActualizaACAsync(ACDto.Data);
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = ACDto.User.UserID,
                    Seccion = "Contratos",
                    Accion = "Actualizar",
                    Descripcion = string.Concat("Se actualiza el contrato " + ACDto.Data.Contract, result.Data.Contract)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            return result.ToActionResult();
        }

    }
}
