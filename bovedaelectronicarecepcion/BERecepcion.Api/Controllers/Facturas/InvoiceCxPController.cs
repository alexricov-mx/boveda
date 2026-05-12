using BERecepcion.Api.Filters;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.Facturas
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class InvoiceCxPController : ControllerBase
    {
        private readonly IInvoiceCxPRepository _InvoiceCxPRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICopadeRepository _copadeRepository;
        private readonly ISAPPIRepository _sapPIRepository;
        private readonly IFacturaElectronicaRepository _facturaElectronicaRepository;

        public InvoiceCxPController(IInvoiceCxPRepository InvoiceCxPRepository, IInvoiceRepository invoiceRepository, ICopadeRepository copadeRepository, ISAPPIRepository sAPPIRepository, IFacturaElectronicaRepository facturaElectronicaRepository)
        {
            _InvoiceCxPRepository = InvoiceCxPRepository;
            _invoiceRepository = invoiceRepository;
            _copadeRepository = copadeRepository;
            _sapPIRepository = sAPPIRepository;
            _facturaElectronicaRepository = facturaElectronicaRepository;
        }

        [HttpGet("GetInvoiceCxPAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<InvoiceCxPList>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceCxPAsync(int pageSize, int pageNum = 1)
        {
            try
            {
                return Ok(await _InvoiceCxPRepository.GetInvoiceCxPAsync(pageSize, pageNum = 1));
            }
            catch (Exception ex)
            {
                Log.Error("GetAdefas: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        //[HttpGet("GetInvoiceCxPAPAsync")]
        //[ProducesResponseType(typeof(IEnumerable<CXPDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GetInvoiceCxPAPAsync()
        //{
        //    try
        //    {
        //        return Ok(await _InvoiceCxPRepository.GetInvoiceCxPAPAsync());
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("GetInvoiceCxPAPAsync: {error}", ex.ToString());
        //        return Problem(null, null, 500, "Error interno", null);
        //    }
        //}

        [HttpPost("PostInvoiceCxPAsync")]
        [ProducesResponseType(typeof(DataResult<CXPDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<CXPDto>> PostInvoiceCxPAsync(DataResult<CXPDto> invoiceCxP)
        {
            DataResult<CXPDto> resultItem = new DataResult<CXPDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceIDByReception(invoiceCxP.Data.Organismo, invoiceCxP.Data.Entrada, invoiceCxP.Data.Ejercicio);
                if(invoice.Data == null)
                {
                    resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                    resultItem.Message = "No existe una factura asociada";
                    return resultItem;
                }

                var dataInvoiceCxPDto = new InvoiceCxPDto()
                {
                    InvoiceId = invoice.Data.InvoiceId,
                    UserId = invoiceCxP.User.UserID                        
                };

                var dataEstatus = new InvoiceEstatusDto
                {
                    InvoiceId = invoice.Data.InvoiceId                        
                };

                var invoiceDocumentoSAP = new InvoiceSapDocumentDto()
                {
                    InvoiceId = invoice.Data.InvoiceId                        
                };

                var result = await _sapPIRepository.PostCxP(invoiceCxP.Data);
                if (result.Status != System.Net.HttpStatusCode.OK)
                {
                    // Error de comunicacion - Internal Server Error
                    // Estatus 210
                    dataEstatus.Estatus = "210";
                    await _facturaElectronicaRepository.SetInvoiceEstatus(dataEstatus);
                    dataInvoiceCxPDto.Res = result.Message;                        
                    var InvoiceCxP = await _facturaElectronicaRepository.SaveInvoiceCxP(dataInvoiceCxPDto);
                    resultItem.Message = dataInvoiceCxPDto.Res;
                    return resultItem;
                }
                else
                {
                    // Ok
                    if (result.Data.DocumentoSAP != null && !string.IsNullOrWhiteSpace(result.Data.DocumentoSAP))
                    {
                        // CxP exitosa, tenemos el DocumentoSAP
                        // Estatus 200 - Finalizado
                        dataEstatus.Estatus = "200";                            
                        await _facturaElectronicaRepository.SetInvoiceEstatus(dataEstatus);
                        // Actualizamos el registro en Invoice con el Documento SAP
                        invoiceDocumentoSAP.SapDocument = result.Data.DocumentoSAP;                            
                        await _facturaElectronicaRepository.SetInvoiceSapDocument(invoiceDocumentoSAP);
                        resultItem.Data.DocumentoSAP = result.Data.DocumentoSAP;
                        return resultItem;
                    }
                    else
                    {
                        // Error de Negocio
                        // Estatus 220
                        dataEstatus.Estatus = "220";
                        await _facturaElectronicaRepository.SetInvoiceEstatus(dataEstatus);
                        dataInvoiceCxPDto.Res = result.Data.Mensaje;                            
                        await _facturaElectronicaRepository.SaveInvoiceCxP(dataInvoiceCxPDto);
                        resultItem.Message = dataInvoiceCxPDto.Res;
                        return resultItem; ;
                    }
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = ex.Message;
                return resultItem;
            }
        }


        [HttpGet("GetInvoiceCxPFiltroAsync")]
        [ProducesResponseType(typeof(IEnumerable<InvoiceCxPList>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceCxPFiltroAsync(string Filtro, int pageSize, int pageNum = 1)
        {
            try
            {
                return Ok(await _InvoiceCxPRepository.GetInvoiceCxPFiltroAsync(Filtro, pageSize, pageNum));
            }
            catch (Exception ex)
            {
                Log.Error("GetInvoiceCxPAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }


        }
    }
}
