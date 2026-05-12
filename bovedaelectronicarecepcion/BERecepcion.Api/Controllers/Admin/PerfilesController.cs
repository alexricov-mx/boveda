using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Exceptions;
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
    public class PerfilesController : ControllerBase
    {
        private readonly IPerfilesRepository _perfilesRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        public PerfilesController(IPerfilesRepository perfilesRepository, IBitacoraRepository bitacoraRepository)
        {
            _perfilesRepository = perfilesRepository;
            _bitacoraRepository = bitacoraRepository;
        }
        [HttpGet("GetPerfilesAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<ProfilesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPerfilesAsync(int pageSize, int pageNum = 1, string search = null)
        {
            var result = await _perfilesRepository.GetPerfilesAsync(pageSize, pageNum, search);
            return result.ToActionResult();
        }
        [HttpGet("GetAllPerfilesAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<ProfilesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPerfilesAsync(string search)
        {
            var result = await _perfilesRepository.GetAllPerfilesAsync(search);
            return result.ToActionResult();
        }
        [HttpGet("GetPerfilAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<ProfilesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPerfilAsync(Guid ProfileID)
        {
            var result = await _perfilesRepository.GetPerfilAsync(ProfileID);
            return result.ToActionResult();
        }
        [HttpPost("BorraPerfilAsync")]
        [ProducesResponseType(typeof(DataResult<ProfilesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BorraPerfilAsync([FromBody] DataResult<ProfilesDto> dto)
        {
            var result = await _perfilesRepository.BorraPerfilAsync(dto.Data.ProfileID);
            
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = dto.User.UserID,
                    Seccion = "Perfiles",
                    Accion = "Eliminar",
                    Descripcion = string.Concat("Se borra el perfil ", result.Data.Name)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            
            return result.ToActionResult();
        }
        [HttpPost("InsertaPerfilAsync")]
        [ProducesResponseType(typeof(DataResult<ProfilesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaPerfilAsync([FromBody] DataResult<ProfilesDto> dto)
        {
            var result = await _perfilesRepository.InsertaPerfilAsync(dto.Data);
            
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = dto.User.UserID,
                    Seccion = "Perfiles",
                    Accion = "Nuevo",
                    Descripcion = string.Concat("Se crea el perfil ", dto.Data.Name)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            
            return result.ToActionResult();
        }
        [HttpPost("ActualizaPerfilAsync")]
        [ProducesResponseType(typeof(DataResult<ProfilesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaPerfilAsync([FromBody] DataResult<ProfilesDto> dto)
        {
            var result = await _perfilesRepository.ActualizaPerfilAsync(dto.Data);
            
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = dto.User.UserID,
                    Seccion = "Perfiles",
                    Accion = "Actualizar",
                    Descripcion = string.Concat("Se actualiza el perfil ", dto.Data.Name)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            
            return result.ToActionResult();
        }
    }
}
