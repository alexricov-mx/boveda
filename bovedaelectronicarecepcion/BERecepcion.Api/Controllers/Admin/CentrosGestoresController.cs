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
    public class CentrosGestoresController : ControllerBase
    {
        private readonly ICentrosGestoresRepository _centrosGestoresRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        public CentrosGestoresController(ICentrosGestoresRepository centrosGestoresRepository, IBitacoraRepository bitacoraRepository)
        {
            _centrosGestoresRepository = centrosGestoresRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        [HttpGet("GetCGAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<ManagementCentersDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCGAsync(int pageSize, int pageNum = 1, string search = null, bool esDescarga = false)
        {
            var result = await _centrosGestoresRepository.GetCGAsync(pageSize, pageNum, search, esDescarga);
            return result.ToActionResult();
        }
        [HttpGet("GetCGByIdAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<ManagementCentersDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCGByIdAsync(Guid ManagementCenterID)
        {
            var result = await _centrosGestoresRepository.GetCGByIdAsync(ManagementCenterID);
            return result.ToActionResult();
        }
        [HttpPost("InsertaCGAsync")]
        [ProducesResponseType(typeof(DataResult<ManagementCentersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaCGAsync([FromBody] DataResult<ManagementCentersDto> cGDto)
        {
            var result = await _centrosGestoresRepository.InsertaCGAsync(cGDto.Data);
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = cGDto.User.UserID,
                    Seccion = "Centros Gestores",
                    Accion = "Alta",
                    Descripcion = string.Concat("Se da de alta el centro gestor " + cGDto.Data.Number)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            return result.ToActionResult();
        }

        [HttpPost("ActualizaCGAsync")]
        [ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaCGAsync([FromBody] DataResult<ManagementCentersDto> cGDto)
        {
            var result = await _centrosGestoresRepository.ActualizaCGAsync(cGDto.Data);
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = cGDto.User.UserID,
                    Seccion = "Centros Gestores",
                    Accion = "Actualizar",
                    Descripcion = string.Concat("Se actualiza el Centro Gestor " + cGDto.Data.Number)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            return result.ToActionResult();
        }

        [HttpPost("BorraCGAsync")]
        [ProducesResponseType(typeof(DataResult<ManagementCentersDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BorraCGAsync([FromBody] DataResult<ManagementCentersDto> dto)
        {
            var result = await _centrosGestoresRepository.BorraCGAsync(dto.Data.ManagementCenterID);
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = dto.User.UserID,
                    Seccion = "Centros Gestores",
                    Accion = "Eliminar",
                    Descripcion = string.Concat("Se elimina el centro gestor " + result.Data.Number)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            return result.ToActionResult();
        }








        [HttpGet("BusquedaCGUsuarioAsync/{ficha}")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<ManagementCentersDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BusquedaCGUsuarioAsync(string ficha)
        {
            var result = await _centrosGestoresRepository.BusquedaCGUsuario(ficha);
            return result.ToActionResult();
        }

        [HttpPost("CargaCentrosGAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<ManagementCentersDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CargaCentrosGAsync([FromBody] DataResult<IEnumerable<ManagementCentersDto>> CentrosGestores)
        {
            var result = await _centrosGestoresRepository.CargaCentrosGAsync(CentrosGestores.Data);
            return result.ToActionResult();
        }


    }
}
