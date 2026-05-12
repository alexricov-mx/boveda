using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface ICFDIAPRepository
    {
        InvoiceDto setInvoiceDto(InvoiceResultDto comprobante, AnaliticoPagoDto analitico, string tipoComprobante, bool isElectronicReception, bool isCopade, Guid UserId, string ViaPago);
        CXPDto setApiCXPDto(InvoiceResultDto comprobante, AnaliticoPagoDto analitico, string tipoComprobante, bool isElectronicReception, bool isCopade, string token);
        DateTime convertToDate(string d);
        Task<InvoiceResultDto> SaveInvoice(InvoiceResultDto comprobante);
        Task<InvoiceResultDto> processSaveInvoice(InvoiceResultDto comprobante);
        string setSourceDocument(ComprobanteBE comprobante, string originalAnaliticoId);
    }
}
