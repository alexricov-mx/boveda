using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public ReactivaProcesosController(IReactivaProcesosRepository reactivaProcesosRepository, ISAPPIRepository sAPPIRepository)
        {
            _reactivaProcesosRepository = reactivaProcesosRepository;
            _sapPIRepository = sAPPIRepository;
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

    }
}
