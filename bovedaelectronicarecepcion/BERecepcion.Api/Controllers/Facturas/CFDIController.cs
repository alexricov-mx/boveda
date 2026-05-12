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
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using BERecepcion.Core.SAT.Interfaces.Repositories;

namespace BERecepcion.Api.Controllers.Facturas
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class CFDIController : Controller
    {
        private readonly ICopadeRepository _copadeRepository;
        private readonly ISAPPIRepository _sapPIRepository;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly IDocumentosRepository _documentosRepository;
        private readonly ICorreoRepository _correoRepository;
        private readonly IUsuariosRepository _usuariosRepository;
        private readonly IESignRepository _eSignRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly ICFDIRepository _cfdiRepository;
        private readonly IFacturaElectronicaRepository _facturaElectronicaRepository;
        private readonly IAdefasRepository _adefasRepository;
        private readonly ISATRepository _sATRepository;
        private readonly ICFDIAPRepository _cFDIAPRepository;

        public CFDIController(ICopadeRepository copadeRepository, ISAPPIRepository sAPPIRepository, IHostEnvironment env, IConfiguration configuration, IDocumentosRepository documentosRepository,
            ICorreoRepository correoRepository, IUsuariosRepository usuariosRepository, IESignRepository eSignRepository, IBitacoraRepository bitacoraRepository,
            ICFDIRepository cfdiRepository, IFacturaElectronicaRepository facturaElectronicaRepository, IAdefasRepository adefasRepository,
            ISATRepository sATRepository, ICFDIAPRepository cFDIAPRepository)
        {
            _copadeRepository = copadeRepository;
            _sapPIRepository = sAPPIRepository;
            _env = env;
            _configuration = configuration;
            _documentosRepository = documentosRepository;
            _correoRepository = correoRepository;
            _usuariosRepository = usuariosRepository;
            _eSignRepository = eSignRepository;
            _bitacoraRepository = bitacoraRepository;
            _cfdiRepository = cfdiRepository;
            _facturaElectronicaRepository = facturaElectronicaRepository;
            _adefasRepository = adefasRepository;
            _sATRepository = sATRepository;
            _cFDIAPRepository = cFDIAPRepository;
        }

        [HttpGet("GetValidationErrorCatalog")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<ValidationError>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetValidationErrorCatalog()
        {
            try
            {
                return Ok(await _cfdiRepository.GetValidationErrorCatalog());

            }
            catch (Exception ex)
            {
                Log.Error("CFDI: GetValidationErrorCatalog {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("GetOrganismRFC")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<string> GetOrganismRFC([FromBody] string organismId)
        {
            string result = "";
            try
            {
                result = await _cfdiRepository.GetOrganismRFC(organismId);
            }
            catch (Exception)
            {
                result = "";
            }
            return result;
        }

        [HttpPost("SaveInvoice")]
        [ProducesResponseType(typeof(ComprobanteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ComprobanteDto> SaveInvoice(ComprobanteDto comprobante)
        {
            var result = await _cfdiRepository.SaveInvoice(comprobante);
            return result;
        }

        [HttpPost("SaveInvoiceMultiple")]
        [ProducesResponseType(typeof(IEnumerable<ComprobanteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IEnumerable<ComprobanteDto>> SaveInvoiceMultiple(IEnumerable<ComprobanteDto> comprobantes)
        {
            var result = await _cfdiRepository.SaveInvoiceMultiple(comprobantes);
            return result;
        }

        [HttpGet("GetPendingInvoice")]
        [ProducesResponseType(typeof(DataResult<List<PendingInvoiceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPendingInvoice()
        {
            try
            {
                return Ok(await _cfdiRepository.GetPendingInvoice());

            }
            catch (Exception ex)
            {
                Log.Error("CFDI: GetPendingInvoice {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("ReprocessInvoice")]
        [ProducesResponseType(typeof(DataResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReprocessInvoice()
        {
            try
            {
                return Ok(await _cfdiRepository.ReprocessInvoice());

            }
            catch (Exception ex)
            {
                Log.Error("CFDI: ReprocessInvoice {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("SaveInvoiceAP")]
        [ProducesResponseType(typeof(InvoiceResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<InvoiceResultDto> SaveInvoiceAP(InvoiceResultDto comprobante)
        {
            comprobante.comprobante = JsonConvert.DeserializeObject<ComprobanteBE>(comprobante.ComprobanteBEString);
            var result = await _cFDIAPRepository.SaveInvoice(comprobante);
            return result;
        }
    }
}
