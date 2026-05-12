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
using BERecepcion.Core.Models;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Facturas.Dto;

namespace BERecepcion.Api.Controllers.Facturas
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class FacturaElectronicaController : Controller
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
        private readonly IFacturaElectronicaRepository _facturaElectronicaRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        public FacturaElectronicaController(ICopadeRepository copadeRepository, ISAPPIRepository sAPPIRepository, IHostEnvironment env, IConfiguration configuration, IDocumentosRepository documentosRepository,
            ICorreoRepository correoRepository, IUsuariosRepository usuariosRepository, IESignRepository eSignRepository, IBitacoraRepository bitacoraRepository,
            IFacturaElectronicaRepository facturaElectronicaRepository, IInvoiceRepository invoiceRepository)
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
            _facturaElectronicaRepository = facturaElectronicaRepository;
            _invoiceRepository = invoiceRepository;
        }

        [HttpPost("GetInvoice")]
        [ProducesResponseType(typeof(DataResult<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoice([FromBody] DataResult<InvoiceDto> dto)
        {
            try
            {
                return Ok(await _facturaElectronicaRepository.GetInvoice(dto.Data.DocumentoBEId, dto.Data.IsCopade));
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: GetInvoice {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("GetInvoiceFullData")]
        [ProducesResponseType(typeof(DataResult<InvoiceFullDataDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceFullData([FromBody] DataResult<InvoiceFullDataDto> dto)
        {
            try
            {
                var result = await _facturaElectronicaRepository.GetInvoiceFullData(dto.Data.invoiceDto.DocumentoBEId, dto.Data.invoiceDto.IsCopade);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: GetInvoiceFullData {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("SaveInvoice")]
        [ProducesResponseType(typeof(DataResult<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveInvoice([FromBody] DataResult<InvoiceDto> dto)
        {
            try
            {
                var result = await _facturaElectronicaRepository.SaveInvoice(dto.Data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: SaveInvoice {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("SetInvoiceEstatus")]
        [ProducesResponseType(typeof(DataResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetInvoiceEstatus(DataResult<InvoiceEstatusDto> dto)
        {
            try
            {
                var result = await _facturaElectronicaRepository.SetInvoiceEstatus(dto.Data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: SetInvoiceEstatus {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("SetInvoiceLastStatus")]
        [ProducesResponseType(typeof(DataResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetInvoiceLastStatus(DataResult<InvoiceEstatusDto> dto)
        {
            try
            {
                var result = await _facturaElectronicaRepository.SetInvoiceLastStatus(dto.Data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: SetInvoiceLastStatus {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("SetInvoiceSapDocument")]
        [ProducesResponseType(typeof(DataResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetInvoiceSapDocument(DataResult<InvoiceSapDocumentDto> dto)
        {
            try
            {
                var result = await _facturaElectronicaRepository.SetInvoiceSapDocument(dto.Data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: SetInvoiceSapDocument {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("SaveInvoiceCxP")]
        [ProducesResponseType(typeof(DataResult<InvoiceCxPDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveInvoiceCxP([FromBody] DataResult<InvoiceCxPDto> dto)
        {
            try
            {
                return Ok(await _facturaElectronicaRepository.SaveInvoiceCxP(dto.Data));
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: SaveInvoiceCxP {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("SaveInvoiceNotaCredito")]
        [ProducesResponseType(typeof(DataResult<InvoiceNotaCreditoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveInvoiceNotaCredito([FromBody] DataResult<InvoiceNotaCreditoDto> dto)
        {
            try
            {
                return Ok(await _facturaElectronicaRepository.SaveInvoiceNotaCredito(dto.Data));
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: SaveInvoiceNotaCredito {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("GetInvoiceCxPList")]
        [ProducesResponseType(typeof(DataResult<InvoiceCxPListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceCxPList(DataResult<InvoiceCxPListDto> dto)
        {
            try
            {
                var result = await _facturaElectronicaRepository.GetInvoiceCxPList(dto.Data.InvoiceId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: SaveInvoiceCxP {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("GetInvoiceByUUID")]
        [ProducesResponseType(typeof(DataResult<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceByUUID([FromBody] DataResult<InvoiceDto> dto)
        {
            try
            {
                return Ok(await _facturaElectronicaRepository.GetInvoiceByUUID(dto.Data.Uuid, dto.Data.Folio));
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: GetInvoiceByUUID {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("GetInvoiceNotaCreditoByUUID")]
        [ProducesResponseType(typeof(DataResult<InvoiceNotaCreditoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceNotaCreditoByUUID([FromBody] DataResult<InvoiceNotaCreditoDto> dto)
        {
            try
            {
                return Ok(await _facturaElectronicaRepository.GetInvoiceNotaCreditoByUUID((Guid)dto.Data.Uuid, dto.Data.Folio));
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: GetInvoiceNotaCreditoByUUID {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("GetInvoiceByReception")]
        [ProducesResponseType(typeof(DataResult<InvoiceByReceptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceByReception([FromBody] DataResult<InvoiceByReceptionDto> dto)
        {
            try
            {
                return Ok(await _invoiceRepository.GetInvoiceByReception(dto.Data.organismo, dto.Data.reception, dto.Data.ejercicio));
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: GetInvoiceByReception {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("GetInvoiceNotaCreditoByReception")]
        [ProducesResponseType(typeof(DataResult<InvoiceNotaCreditoByReceptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceNotaCreditoByReception([FromBody] DataResult<InvoiceNotaCreditoByReceptionDto> dto)
        {
            try
            {
                return Ok(await _facturaElectronicaRepository.GetInvoiceNotaCreditoByReception(dto.Data));
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: GetInvoiceNotaCreditoByReception {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("SaveInvoiceGral")]
        [ProducesResponseType(typeof(DataResult<InvoiceSaveGralDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveInvoiceGral([FromBody] DataResult<InvoiceSaveGralDto> dto)
        {
            try
            {
                var result = await _facturaElectronicaRepository.SaveInvoiceGral(dto.Data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("FacturaElectronicaController: SaveInvoiceGral {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
    }
}
