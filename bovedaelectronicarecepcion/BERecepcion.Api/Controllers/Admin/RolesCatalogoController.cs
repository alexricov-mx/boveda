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
    public class RolesCatalogoController : ControllerBase
    {
        private readonly IRolesCatalogoRepository _rolesCatalogoRepository;
        public RolesCatalogoController(IRolesCatalogoRepository rolesCatalogoRepository)
        {
            _rolesCatalogoRepository = rolesCatalogoRepository;

        }
        [HttpGet("GetRolesAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<RolesCatalogoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRolesAsync(int pageSize, int pageNum = 1)
        {
            var result = await _rolesCatalogoRepository.GetRolesAsync();
            return result.ToActionResult();
        }
    }
}
