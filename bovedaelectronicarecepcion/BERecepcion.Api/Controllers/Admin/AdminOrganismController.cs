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
    public class AdminOrganismController : ControllerBase
    {


        private readonly IOrganismRepository _organismRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        public AdminOrganismController(IOrganismRepository organismRepository, IBitacoraRepository bitacoraRepository)
        {
            _organismRepository = organismRepository;
            _bitacoraRepository = bitacoraRepository;
        }



        [HttpGet("GetOrganismAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<OrganismDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrganismAsync()
        {
            var result = await _organismRepository.GetOrganismAsync();
            return result.ToActionResult();
        }

        [HttpPost("{Id}")]
        [ProducesResponseType(typeof(DataResult<OrganismDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizaOrganismAsync(Guid Id, [FromBody] OrganismDto organismDto)
        {
            var result = await _organismRepository.ActualizaOrganismAsync(organismDto);
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = organismDto.usuarioId,
                    Seccion = "CatalogoOrganismos",
                    Accion = "Modificar",
                    Descripcion = string.Concat("Se actualizo el Organismo ", organismDto.Clave)
                };
                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }
            return result.ToActionResult();
        }
    }
}
