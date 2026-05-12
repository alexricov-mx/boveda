using BERecepcion.Api.ModelBinding;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BERecepcion.Api.HtmlHelpers;
using iText.Html2pdf;
using System.Globalization;
using System.Drawing;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Facturas.Dto;

namespace BERecepcion.Api.Controllers.Consulta
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class ConsultasController : Controller
    {
        private readonly ICopadeRepository _copadeRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly IConsultasRepository _consultasRepository;
        private readonly ISOEstimationRepository _ordenSurtimientoRepository;

        public ConsultasController(ICopadeRepository copadeRepository, ISOEstimationRepository ordenSurtimientoRepository,
            IBitacoraRepository bitacoraRepository, IConsultasRepository consultasRepository)
        {
            _copadeRepository = copadeRepository;
            _ordenSurtimientoRepository = ordenSurtimientoRepository;
            _bitacoraRepository = bitacoraRepository;
            _consultasRepository = consultasRepository;
        }

        [HttpGet("GetListaFiltroCopadesAsync")]
        [ProducesResponseType(typeof(DataResult<List<CopadeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListaFiltroCopadesAsync(string UserID, int pageSize, int pageNum = 1, string search = null)
        {
            try
            {
                return Ok(await _copadeRepository.GetListaFiltroCopadesAsync(UserID, pageSize, pageNum, search));
            }
            catch (Exception ex)
            {
                Log.Error("CopadeBancario: GetListaFiltroCopadesAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetListaCopadeBancarioAsync")]
        [ProducesResponseType(typeof(DataResult<List<CopadeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListaCopadeBancarioAsync(string start, string end, string search, string UserID, string claveOrganismo, string creditorNumber, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            try
            {
                DateTime _start = new DateTime(Convert.ToInt32(start.Substring(0, 4)), Convert.ToInt32(start.Substring(4, 2)), Convert.ToInt32(start.Substring(6, 2)));
                DateTime _end = new DateTime(Convert.ToInt32(end.Substring(0, 4)), Convert.ToInt32(end.Substring(4, 2)), Convert.ToInt32(end.Substring(6, 2)));
                return Ok(await _copadeRepository.GetListaCopadeBancarioAsync(_start, _end, search, UserID, claveOrganismo, creditorNumber, pageSize, pageNum, esDescarga));
            }
            catch (Exception ex)
            {
                Log.Error("CopadeBancario: GetListaCopadeBancarioAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
        [HttpGet("GetListaRechazosAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<ReporteRechazosDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListaRechazosAsync(string start, string end, string search, Guid userId, int source, int pageSize, int pageNum = 1)
        {
            try
            {
                DateTime _start = new DateTime(Convert.ToInt32(start.Substring(0, 4)), Convert.ToInt32(start.Substring(4, 2)), Convert.ToInt32(start.Substring(6, 2)));
                DateTime _end = new DateTime(Convert.ToInt32(end.Substring(0, 4)), Convert.ToInt32(end.Substring(4, 2)), Convert.ToInt32(end.Substring(6, 2)));
                return Ok(await _bitacoraRepository.GetListaRechazosAsync(_start, _end, search, userId, source, pageSize, pageNum));

            }
            catch (Exception ex)
            {
                Log.Error("ConsultaRechazos: GetListaRechazosAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetListaPaymentScheduleAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<ReportePaymentScheduleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListaPaymentScheduleAsync(string start, string end, string search, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            try
            {
                DateTime _start = new DateTime(Convert.ToInt32(start.Substring(0, 4)), Convert.ToInt32(start.Substring(4, 2)), Convert.ToInt32(start.Substring(6, 2)));
                DateTime _end = new DateTime(Convert.ToInt32(end.Substring(0, 4)), Convert.ToInt32(end.Substring(4, 2)), Convert.ToInt32(end.Substring(6, 2)));
                return Ok(await _consultasRepository.GetListaPaymentScheduleAsync(_start, _end, search, userId, pageSize, pageNum, esDescarga));

            }
            catch (Exception ex)
            {
                Log.Error("ConsultaPaymentSchedule: GetListaPaymentScheduleAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetListaPaymentListAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<ReportePaymentListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListaPaymentListAsync(string start, string end, string filtro, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            try
            {
                DateTime _start = new DateTime(Convert.ToInt32(start.Substring(0, 4)), Convert.ToInt32(start.Substring(4, 2)), Convert.ToInt32(start.Substring(6, 2)));
                DateTime _end = new DateTime(Convert.ToInt32(end.Substring(0, 4)), Convert.ToInt32(end.Substring(4, 2)), Convert.ToInt32(end.Substring(6, 2)));
                if (filtro == null) filtro = "";
                return Ok(await _consultasRepository.GetListaPaymentListAsync(_start, _end, filtro, userId, pageSize, pageNum, esDescarga));

            }
            catch (Exception ex)
            {
                Log.Error("ConsultaPaymentSchedule: GetListaPaymentScheduleAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetListaEstimacionesBancariasAsync")]
        [ProducesResponseType(typeof(DataResult<List<SOEstimationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListaEstimacionesBancariasAsync(string start, string end, string search, string UserID, string claveOrganismo, string creditorNumber, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            try
            {
                DateTime _start = new DateTime(Convert.ToInt32(start.Substring(0, 4)), Convert.ToInt32(start.Substring(4, 2)), Convert.ToInt32(start.Substring(6, 2)));
                DateTime _end = new DateTime(Convert.ToInt32(end.Substring(0, 4)), Convert.ToInt32(end.Substring(4, 2)), Convert.ToInt32(end.Substring(6, 2)));
                return Ok(await _ordenSurtimientoRepository.GetListaEstimacionesBancariasAsync(_start, _end, search, UserID, claveOrganismo, creditorNumber, pageSize, pageNum, esDescarga));
            }
            catch (Exception ex)
            {
                Log.Error("OrdenSurtimientoBancario: GetListaOrdenesAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetListaOrdenesBancariasAsync")]
        [ProducesResponseType(typeof(DataResult<List<SOEstimationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListaOrdenesBancariasAsync(string start, string end, string search, string UserID, string claveOrganismo, string creditorNumber, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            try
            {
                DateTime _start = new DateTime(Convert.ToInt32(start.Substring(0, 4)), Convert.ToInt32(start.Substring(4, 2)), Convert.ToInt32(start.Substring(6, 2)));
                DateTime _end = new DateTime(Convert.ToInt32(end.Substring(0, 4)), Convert.ToInt32(end.Substring(4, 2)), Convert.ToInt32(end.Substring(6, 2)));
                return Ok(await _ordenSurtimientoRepository.GetListaOrdenesBancariasAsync(_start, _end, search, UserID, claveOrganismo, creditorNumber, pageSize, pageNum, esDescarga));
            }
            catch (Exception ex)
            {
                Log.Error("OrdenSurtimientoBancario: GetListaOrdenesAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("GetEstadoFacturasAsync")]
        [ProducesResponseType(typeof(DataResult<List<EstadoFacturaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEstadoFacturasAsync(string start, string end, string search, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false)
        {
            try
            {
                DateTime _start = new DateTime(Convert.ToInt32(start.Substring(0, 4)), Convert.ToInt32(start.Substring(4, 2)), Convert.ToInt32(start.Substring(6, 2)));
                DateTime _end = new DateTime(Convert.ToInt32(end.Substring(0, 4)), Convert.ToInt32(end.Substring(4, 2)), Convert.ToInt32(end.Substring(6, 2)));
                return Ok(await _consultasRepository.GetEstadoFacturasAsync(_start, _end, search, userId, pageSize, pageNum, esDescarga));
            }
            catch (Exception ex)
            {
                Log.Error("Consultas: GetEstadoFacturasAsync {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
    }
}
