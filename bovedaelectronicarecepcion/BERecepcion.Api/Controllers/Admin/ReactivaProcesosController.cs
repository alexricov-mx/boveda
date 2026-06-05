using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Api.Infrastructure.Auth;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class ReactivaProcesosController : ControllerBase
    {
        private readonly IReactivaProcesosRepository _reactivaProcesosRepository;
        private readonly ISAPPIRepository _sapPIRepository;
        private readonly IReactivaProcesoServiceAsync _reactivaProcesoServiceAsync;
        public ReactivaProcesosController(IReactivaProcesosRepository reactivaProcesosRepository, ISAPPIRepository sAPPIRepository, IReactivaProcesoServiceAsync reactivaProcesoServiceAsync)
        {
            _reactivaProcesosRepository = reactivaProcesosRepository;
            _sapPIRepository = sAPPIRepository;
            _reactivaProcesoServiceAsync = reactivaProcesoServiceAsync;
        }


        [HttpGet("{SAPOrder}")]
        [ProducesResponseType(typeof(ReactivaProcesosDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEnvioFirmaAsync(string SAPOrder)
        {
            var result = await _reactivaProcesosRepository.GetEnvioFirmaAsync(SAPOrder);
            return result.ToActionResult();
        }

        [HttpGet("GetEnvioSAPFirma/{SAPOrder}")]
        public async Task<IActionResult> GetEnvioSAPFirmaAsync(
            [FromRoute] string SAPOrder,
            CancellationToken cancellationToken
            )
        {
            var result = await _reactivaProcesoServiceAsync.GetSAPFirmaAsync(SAPOrder, cancellationToken);
            return result.ToActionResult(this);
        }
        [HttpPost]
        [ProducesResponseType(typeof(JsonRP), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PIEnvioReactivaProcesosAsync([FromBody] DataResult<JsonRP> datosRect)
        {
            var result = await _reactivaProcesosRepository.PIEnvioReactivaProcesosAsync(datosRect);
            return result.ToActionResult();
        }

        [HttpGet("GetReactivaProcesos/{OrderSAP}")]
        [ProducesResponseType(typeof(ReactivaProcesosDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReactivaProcesosAsync(string OrderSAP)
        {
            var result = await _reactivaProcesosRepository.GetReactivaProcesosAsync(OrderSAP);
            return result.ToActionResult();
        }


        [HttpGet("GetReactivaProcesosBySAPOrder/{OrderSAP}")]
        public async Task<IActionResult> GetReactivaProcesosBySAPOrderAsync(
            [FromRoute] string OrderSAP,
            CancellationToken cancellationToken
            )
        {
            var result = await _reactivaProcesoServiceAsync.GetReactivaProcessAsync(OrderSAP, cancellationToken);
            return result.ToActionResult(this);
        }

    }
}
