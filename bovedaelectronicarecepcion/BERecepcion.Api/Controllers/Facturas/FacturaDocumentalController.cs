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
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Core.Facturas.Dto;

namespace BERecepcion.Api.Controllers.Facturas
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class FacturaDocumentalController : Controller
    {
        private readonly IFacturaDocumentalAPSinCFDIRepository _facturaDocumentalAPSinCFDIRepository;
        private readonly IFacturaDocumentalAPConCFDIRepository _facturaDocumentalAPConCFDIRepository;
        private readonly IFacturaDocumentalSinCFDIRepository _facturaDocumentalSinCFDIRepository;
        private readonly IFacturaDocumentalConCFDIRepository _facturaDocumentalConCFDIRepository;

        public FacturaDocumentalController(IFacturaDocumentalAPSinCFDIRepository facturaDocumentalAPSinCFDIRepository,
            IFacturaDocumentalAPConCFDIRepository facturaDocumentalAPConCFDIRepository,
            IFacturaDocumentalSinCFDIRepository facturaDocumentalSinCFDIRepository,
            IFacturaDocumentalConCFDIRepository facturaDocumentalConCFDIRepository)
        {
            _facturaDocumentalAPSinCFDIRepository = facturaDocumentalAPSinCFDIRepository;
            _facturaDocumentalAPConCFDIRepository = facturaDocumentalAPConCFDIRepository;
            _facturaDocumentalSinCFDIRepository = facturaDocumentalSinCFDIRepository;
            _facturaDocumentalConCFDIRepository = facturaDocumentalConCFDIRepository;
        }

        [HttpPost("SaveInvoice")]
        [ProducesResponseType(typeof(InvoiceResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<InvoiceResultDto> SaveInvoice(InvoiceResultDto dto)
        {
            var result = await _facturaDocumentalAPSinCFDIRepository.SaveInvoice(dto.Organismo, dto.IdDocumento, dto.RFCReceptor, dto.Correo, dto.InvoiceDto, dto.User); ;
            return result;
        }

        [HttpPost("SaveInvoiceComprobante")]
        [ProducesResponseType(typeof(InvoiceResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<InvoiceResultDto> SaveInvoiceComprobante(InvoiceResultDto dto)
        {
            dto.comprobante = JsonConvert.DeserializeObject<ComprobanteBE>(dto.ComprobanteBEString);
            var result = await _facturaDocumentalAPConCFDIRepository.SaveInvoice(dto.Organismo, dto.IdDocumento, dto.RFCReceptor, dto.DocumentoBEId, dto.comprobante, dto.User, dto.OriginalXML, dto.ViaPago, dto.ComprobanteOriginal);
            return result;
        }

        [HttpPost("SaveInvoiceSinCFDI")]
        [ProducesResponseType(typeof(InvoiceResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<InvoiceResultDto> SaveInvoiceSinCFDI(InvoiceResultDto dto)
        {
            var result = await _facturaDocumentalSinCFDIRepository.SaveInvoice(dto.Organismo, dto.IdDocumento, dto.comprobante.Addenda.Addenda_Pemex.EJERCICIO, dto.RFCReceptor, dto.Correo, dto.InvoiceDto, dto.User); ;
            return result;
        }

        [HttpPost("SaveInvoiceConCFDI")]
        [ProducesResponseType(typeof(InvoiceResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<InvoiceResultDto> SaveInvoiceConCFDI(InvoiceResultDto dto)
        {
            dto.comprobante = JsonConvert.DeserializeObject<ComprobanteBE>(dto.ComprobanteBEString);
            var result = await _facturaDocumentalConCFDIRepository.SaveInvoice(dto.Organismo, dto.IdDocumento, dto.comprobante.Addenda.Addenda_Pemex.EJERCICIO, dto.RFCReceptor, dto.DocumentoBEId, dto.comprobante, dto.User, dto.OriginalXML, dto.ViaPago, dto.Correo, dto.notasCredito, dto.notasCreditoCFDI, dto.ComprobanteOriginal);
            return result;
        }
    }
}
