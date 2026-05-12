using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.FirmaDocumentos
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class DocumentoFirmadoController : ControllerBase
    {
        private readonly IDocumentoFirmadoRepository _documentoFirmadoRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        public DocumentoFirmadoController(IDocumentoFirmadoRepository documentoFirmadoRepository)
        {
            _documentoFirmadoRepository = documentoFirmadoRepository;
        }
        [HttpGet("GetDocumentoFirmadoAsync")]
        [ProducesResponseType(typeof(DataResult<DocumentoFirmadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentoFirmadoAsync(Guid DocumentoBEId, int? Orden = null)
        {
            try
            {
                return Ok(await _documentoFirmadoRepository.GetDocumentoFirmadoAsync(DocumentoBEId, Orden));
            }
            catch (Exception ex)
            {
                Log.Error("DocumentoFirmado: GetDocumentoFirmadoAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("CreaPaqueteAsync")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreaPaqueteAsync([FromBody] ExternosDto externos, string orden)
        {
            DataResult<DocumentoFirmadoDto> result = new DataResult<DocumentoFirmadoDto>();
            try
            {
                return Ok(await _documentoFirmadoRepository.CreaPaqueteAsync(externos, orden));
            }
            catch (Exception ex)
            {
                Log.Error("DocumentoFirmado: CreaPaqueteAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("ActualizaFirmaAsync")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaFirmaAsync([FromBody] ExternosDto externos, string orden)
        {
            try
            {
                return Ok(await _documentoFirmadoRepository.ActualizaFirmaAsync(externos, orden));
            }
            catch (Exception ex)
            {
                Log.Error("DocumentoFirmado: GetDocumentoFirmadoAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetDocumentoAPFirmadoAsync")]
        [ProducesResponseType(typeof(DataResult<DocumentoFirmadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentoAPFirmadoAsync(Guid DocumentoBEId)
        {
            try
            {
                return Ok(await _documentoFirmadoRepository.GetDocumentoAPFirmadoAsync(DocumentoBEId));
            }
            catch (Exception ex)
            {
                Log.Error("DocumentoFirmado: GetDocumentoAPFirmadoAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
    }
}
