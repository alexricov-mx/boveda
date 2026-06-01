using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.ExpedienteElectronico.Dto;
using BERecepcion.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public ExpedienteElectronicoController(IExpedienteElectronicoServiceAsync expedienteElectronicoService)
        {
            _expedienteElectronicoService = expedienteElectronicoService;
        }

        [HttpGet("GetExpediente")]
        [ProducesResponseType(typeof(ExpedienteEViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExpediente(
            [FromQuery] ExpedienteElectronicoRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _expedienteElectronicoService
                .GetExpedienteAsync(request, cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
