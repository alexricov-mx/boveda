using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BERecepcion.Core.Models;
using BERecepcion.Core.Facturas.Dto;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface IFacturaElectronicaRepository
    {
        Task<DataResult<InvoiceDto>> GetInvoice(Guid DocumentoBEId, bool IsCopade);
        Task<DataResult<InvoiceFullDataDto>> GetInvoiceFullData(Guid DocumentoBEId, bool IsCopade);
        Task<DataResult<InvoiceDto>> SaveInvoice(InvoiceDto dto);
        Task<DataResult<InvoiceEstatusDto>> SetInvoiceEstatus(InvoiceEstatusDto dto);
        Task<DataResult<InvoiceEstatusDto>> SetInvoiceLastStatus(InvoiceEstatusDto dto);
        Task<DataResult<InvoiceSapDocumentDto>> SetInvoiceSapDocument(InvoiceSapDocumentDto dto);
        Task<DataResult<InvoiceCxPDto>> SaveInvoiceCxP(InvoiceCxPDto dto);
        Task<DataResult<InvoiceNotaCreditoDto>> SaveInvoiceNotaCredito(InvoiceNotaCreditoDto dto);
        Task<DataResult<InvoiceCxPListDto>> GetInvoiceCxPList(Guid InvoiceId);
        Task<DataResult<InvoiceDto>> GetInvoiceByUUID(Guid UUID, string folio = "");
        Task<DataResult<InvoiceNotaCreditoDto>> GetInvoiceNotaCreditoByUUID(Guid UUID, string folio = "");
        Task<DataResult<InvoiceNotaCreditoByReceptionDto>> GetInvoiceNotaCreditoByReception(InvoiceNotaCreditoByReceptionDto dto);
        Task<DataResult<InvoiceSaveGralDto>> SaveInvoiceGral(InvoiceSaveGralDto dto);
    }
}
