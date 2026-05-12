using BERecepcion.Api.Filters;
using BERecepcion.Core.Consulta.Dto;
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
    public class ExpedienteElectronicoController : Controller
    {
        private readonly IExpedienteElectronicoRepository _expedienteElectronicoRepository;
        public ExpedienteElectronicoController(IExpedienteElectronicoRepository expedienteElectronicoRepository)
        {
            _expedienteElectronicoRepository = expedienteElectronicoRepository;
        }

        [HttpGet("GetExpediente")]
        [ProducesResponseType(typeof(DataResult<ExpedienteEViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExpediente(string SAPOrder = null, Guid? CopadeID = null, Guid? AnaliticoPagoID = null)
        {
            try
            {
                return Ok(await _expedienteElectronicoRepository.ExpedienteElectronico(SAPOrder, CopadeID, AnaliticoPagoID));
            }
            catch (Exception ex)
            {
                Log.Error("ExpedienteElectronico: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
    }
}
