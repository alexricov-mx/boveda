using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.ExpedienteElectronico.Dto;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces;
using BERecepcion.Infraestructura.Consulta.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.Consulta
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class ExpedienteElectronicoController : ControllerBase
    {
        private readonly IExpedienteElectronicoServiceAsync _expedienteElectronicoService;
        private readonly IExpedienteElectronicoRepositoryAsync _expedienteElectronicoRepository;

        public ExpedienteElectronicoController(
            IExpedienteElectronicoServiceAsync expedienteElectronicoService
            , IExpedienteElectronicoRepositoryAsync expedienteElectronicoRepositoryAsync
            )
        {
            _expedienteElectronicoService = expedienteElectronicoService;
            _expedienteElectronicoRepository = expedienteElectronicoRepositoryAsync;

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

        [HttpGet("GetExpedienteElectronic")]
        public async Task<IActionResult> GetExpedienteElectronic(
            [FromQuery] ExpedienteElectronicoRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _expedienteElectronicoService
                .GetExpedienteAsync(request, cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
