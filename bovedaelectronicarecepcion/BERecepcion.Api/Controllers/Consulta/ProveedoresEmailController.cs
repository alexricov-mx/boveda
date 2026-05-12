using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.Consulta
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class ProveedoresEmailController : Controller
    {
        private readonly IProveedoresEmailRepository _proveedoresemailRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        public ProveedoresEmailController(IProveedoresEmailRepository proveedoresemailRepository, IBitacoraRepository bitacoraRepository)
        {
            _proveedoresemailRepository = proveedoresemailRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        [HttpGet("GetProveedoresEmailGAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<CopadeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProveedoresEmailGAsync(DateTime fechaInicial, DateTime fechaFinal, string search, Guid UserID, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            try
            {
                return Ok(await _proveedoresemailRepository.GetProveedoresEmailGAsync(fechaInicial, fechaFinal, search, UserID, pageSize, pageNum, esDescarga));
            }
            catch (Exception ex)
            {
                Log.Error("GetProveedoresEmailGAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

    }
}
