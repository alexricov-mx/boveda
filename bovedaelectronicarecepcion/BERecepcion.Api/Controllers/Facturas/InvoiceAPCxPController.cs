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
    public class InvoiceAPCxPController : ControllerBase
    {
        private readonly IInvoiceAPCxPRepository _InvoiceAPCxPRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICopadeRepository _copadeRepository;
        private readonly ISAPPIRepository _sapPIRepository;
        private readonly IFacturaElectronicaRepository _facturaElectronicaRepository;
        private readonly IAnaliticoPagoRepository _analiticoPagoRepository;
     

        public InvoiceAPCxPController(IInvoiceAPCxPRepository InvoiceAPCxPRepository, ICopadeRepository copadeRepository, ISAPPIRepository sAPPIRepository, IFacturaElectronicaRepository facturaElectronicaRepository, IAnaliticoPagoRepository analiticoPagoRepository, IInvoiceRepository invoiceRepository)
        {
            _InvoiceAPCxPRepository = InvoiceAPCxPRepository;
            _copadeRepository = copadeRepository;
            _sapPIRepository = sAPPIRepository;
            _facturaElectronicaRepository = facturaElectronicaRepository;
            _analiticoPagoRepository = analiticoPagoRepository;
            _invoiceRepository = invoiceRepository;
        }

        [HttpGet("GetInvoiceAPCxPAsync")]
        [ProducesResponseType(typeof(IEnumerable<InvoiceAPCxPList>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceAPCxPAsync(int pageSize, int pageNum = 1)
        {
            try
            {
                return Ok(await _InvoiceAPCxPRepository.GetInvoiceAPCxPAsync(pageSize, pageNum = 1));
            }
            catch (Exception ex)
            {
                Log.Error("GetInvoiceAPCxPAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }


        }

        //[HttpPost("PostInvoiceCxPAPAsync")]
        //[ProducesResponseType(typeof(DataResult<InvoiceAPCxPList>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<DataResult<InvoiceAPCxPList>> PostInvoiceCxPAPAsync(DataResult<InvoiceAPCxPList> invoiceAPCxP)
        //{
        //    DataResult<InvoiceAPCxPList> resultItem = new DataResult<InvoiceAPCxPList>()
        //    {
        //        Status = System.Net.HttpStatusCode.OK
        //    };
        //    try
        //    {
        //        // nos traemos el analitico de pago                
        //        var analiticoPago = await _analiticoPagoRepository.GetAPByIdAnaliticoAsync(invoiceAPCxP.Data.analiticoPagoID.ToString());

        //        var dataCxP = new CXPDto
        //        {
        //            Organismo = invoiceAPCxP.Data.clave,
        //            Ejercicio = invoiceAPCxP.Data.Exercise,
        //            FechaRecep = invoiceAPCxP.Data.ReceptionDate.ToString("yyyy-MM-dd"),
        //            FechaFactura = invoiceAPCxP.Data.InvoiceDate.ToString("yyyy-MM-dd"),
        //            FechaEmision = invoiceAPCxP.Data.CxpSendDate.ToString("yyyy-MM-dd"),
        //            Factura = invoiceAPCxP.Data.Uuid.ToString(),
        //            ViaPago = invoiceAPCxP.Data.Assignment,
        //            Usuario = invoiceAPCxP.User.Token,
        //            ContratoVigente = invoiceAPCxP.Data.ContratoVigente,
        //            Cliente = invoiceAPCxP.Data.Cliente,
        //            Id_Analitico = invoiceAPCxP.Data.Id_Analitico,
        //            CentroGestor = invoiceAPCxP.Data.CentroGestor,
        //            ImporteFactura = invoiceAPCxP.Data.Total,
        //            ImporteOriginal = analiticoPago.Data.Total,
        //            DiferencialCargo = "0",     // pendiente la regla de diferencial cargo/abono
        //            DiferencialAbono = "0"
        //        };

        //        var dataInvoiceCxPDto = new InvoiceCxPDto
        //        {
        //            InvoiceId = invoiceAPCxP.Data.InvoiceId,
        //            UserId = invoiceAPCxP.User.UserID
        //        };

        //        var dataEstatus = new InvoiceEstatusDto
        //        {
        //            InvoiceId = invoiceAPCxP.Data.InvoiceId
        //        };

        //        var invoiceDocumentoSAP = new InvoiceSapDocumentDto()
        //        {
        //            InvoiceId = invoiceAPCxP.Data.InvoiceId
        //        };

        //        var result = await _sapPIRepository.PostCxP(dataCxP);
        //        if (result.Status != System.Net.HttpStatusCode.OK)
        //        {
        //            // Error de comunicacion - Internal Server Error
        //            // Estatus 210
        //            dataInvoiceCxPDto.Res = result.Message;
        //            var InvoiceCxP = await _facturaElectronicaRepository.SaveInvoiceCxP(dataInvoiceCxPDto);
        //            resultItem.Message = result.Message;
        //            return resultItem;
        //        }
        //        else
        //        {
        //            // Ok
        //            if (result.Data.DocumentoSAP != null)
        //            {
        //                // CxP exitosa, tenemos el DocumentoSAP
        //                // Estatus 200 - Finalizado
        //                dataEstatus.Estatus = "200";
        //                await _facturaElectronicaRepository.SetInvoiceEstatus(dataEstatus);
        //                // Actualizamos el registro en Invoice con el Documento SAP
        //                invoiceDocumentoSAP.SapDocument = result.Data.DocumentoSAP;
        //                await _facturaElectronicaRepository.SetInvoiceSapDocument(invoiceDocumentoSAP);
        //                resultItem.Data.DocumentoSAP = result.Data.DocumentoSAP;
        //                return resultItem;
        //            }
        //            else
        //            {
        //                // Error de Negocio
        //                // Estatus 220
        //                dataInvoiceCxPDto.Res = result.Data.Mensaje;
        //                await _facturaElectronicaRepository.SaveInvoiceCxP(dataInvoiceCxPDto);
        //                resultItem.Message = result.Message;
        //                return resultItem; ;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        resultItem.Message = ex.Message;
        //        return resultItem;
        //    }
        //}

        [HttpPost("PostInvoiceCxPAPAsync")]
        [ProducesResponseType(typeof(DataResult<CXPDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<CXPDto>> PostInvoiceCxPAPAsync(DataResult<CXPDto> invoiceAPCxP)
        {
            DataResult<CXPDto> resultItem = new DataResult<CXPDto>()
            {
                Status = System.Net.HttpStatusCode.OK
            };
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceIDByAnaliticoPago(invoiceAPCxP.Data.Id_Analitico);
                if (invoice.Data == null)
                {
                    resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                    resultItem.Message = "No existe una factura asociada";
                    return resultItem;
                }

                var dataInvoiceCxPDto = new InvoiceCxPDto()
                {
                    InvoiceId = invoice.Data.InvoiceId,
                    UserId = invoiceAPCxP.User.UserID
                };

                var dataEstatus = new InvoiceEstatusDto
                {
                    InvoiceId = invoice.Data.InvoiceId
                };

                var invoiceDocumentoSAP = new InvoiceSapDocumentDto()
                {
                    InvoiceId = invoice.Data.InvoiceId
                };

                var result = await _sapPIRepository.PostCxP(invoiceAPCxP.Data);
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
                    if (result.Data.DocumentoSAP != null)
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

        [HttpGet("GetInvoiceAPCxPFiltroAsync")]
        [ProducesResponseType(typeof(IEnumerable<InvoiceAPCxPList>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoiceAPCxPFiltroAsync(string Filtro, int pageSize, int pageNum = 1)
        {
            try
            {
                return Ok(await _InvoiceAPCxPRepository.GetInvoiceAPCxPFiltroAsync(Filtro, pageSize, pageNum));
            }
            catch (Exception ex)
            {
                Log.Error("GetInvoiceAPCxPFiltroAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }


        }
    }
}
