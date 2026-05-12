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
    public class CatalogosController : ControllerBase
    {

        private readonly ICatalogosRepository _catalogosRepository;
        public CatalogosController(ICatalogosRepository catalogosRepository)
        {
            _catalogosRepository = catalogosRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(OrganismDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CatalogosAsync()
        {
            var result = await _catalogosRepository.GetOrganismosAsync();
            return result.ToActionResult();
        }

        [HttpGet("Profiles")]
        [ProducesResponseType(typeof(ProfilesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfilesAsync()
        {
            var result = await _catalogosRepository.GetProfilesAsync();
            return result.ToActionResult();
        }

    }
}
