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
    public class AdefasController : ControllerBase
    {
        private readonly IAdefasRepository _adefasRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        public AdefasController(IAdefasRepository adefasRepository, IBitacoraRepository bitacoraRepository)
        {
            _adefasRepository = adefasRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(DataResult<IEnumerable<AdefasDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAdefasAsync(int pageSize, int pageNum = 1, string search = null, bool esDescarga = false)
        {
            var result = await _adefasRepository.GetAdefasAsync(pageSize, pageNum, search, esDescarga);
            return result.ToActionResult();
        }

        [HttpPost("InsertaAdefa")]
        [ProducesResponseType(typeof(DataResult<AdefasDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaAdefaAsync([FromBody] DataResult<AdefasDto> adefaDto)
        {
            var result = await _adefasRepository.InsertaAdefaAsync(adefaDto.Data);
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = adefaDto.User.UserID,
                    Seccion = "Adefas",
                    Accion = "Agregar",
                    Descripcion = string.Concat("Se agrega periodo de Adefa " + adefaDto.Data.AnhioFactura)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            return result.ToActionResult();
        }

        [HttpPost("ActualizaAdefaAsync")]
        [ProducesResponseType(typeof(DataResult<AdefasDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaAdefaAsync([FromBody] DataResult<AdefasDto> adefa)
        {
            var result = await _adefasRepository.ActualizaAdefaAsync(adefa.Data);
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = adefa.User.UserID,
                    Seccion = "Adefas",
                    Accion = "Modificar",
                    Descripcion = string.Concat("Se actualizo el periodo de Adefa " + adefa.Data.AnhioFactura)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            return result.ToActionResult();
        }


        [HttpGet("validaPeriodoAdefa/{OrgId}/{AnhiFactura}/{FechaFactura}")]
        [ProducesResponseType(typeof(DataResult<ValidaPeriodoAdefaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetValidaPeriodoAdefaAsync(string OrgId, string AnhiFactura, string FechaFactura)
        {
            var result = await _adefasRepository.GetValidaPeriodoAdefaAsync(OrgId, AnhiFactura, FechaFactura);
            return result.ToActionResult();
        }

        [HttpPost("BorraAdefaAsync")]
        [ProducesResponseType(typeof(DataResult<AdefasDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BorraAdefaAsync([FromBody] DataResult<AdefasDto> adefaDto)
        {
            var result = await _adefasRepository.BorraAdefaAsync(adefaDto.Data.AdefaID);
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = adefaDto.User.UserID,
                    Seccion = "Adefas",
                    Accion = "Eliminar",
                    Descripcion = string.Concat("Se Eliminó el periodo de Adefa " + adefaDto.Data.AnhioFactura)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            return result.ToActionResult();
        }

        [HttpGet("GetAdefaByIdAsync")]
        [ProducesResponseType(typeof(DataResult<AdefasDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAdefaByIdAsync(Guid adefaId)
        {
            var result = await _adefasRepository.GetAdefaByIdAsync(adefaId);
            return result.ToActionResult();
        }
    }
}
