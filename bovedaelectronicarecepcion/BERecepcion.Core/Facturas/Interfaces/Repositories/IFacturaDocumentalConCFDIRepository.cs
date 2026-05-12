using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface IFacturaDocumentalConCFDIRepository
    {
        Task<InvoiceResultDto> SaveInvoice(string Organismo, string Reception, string Exercise, string RFCReceptor, string DocumentoBEId, ComprobanteBE comprobante, UsersDto User, string CFDIXML, string ViaPago, string Correo, IEnumerable<ComprobanteBE> notasCredito, IEnumerable<NotaCreditoCFDIXMLDto> notasCreditoXML, string comprobanteOriginal);
        Task<InvoiceResultDto> ValidateInvoice(string Organismo, string Reception, string Exercise, string RFCReceptor, string DocumentoBEId, ComprobanteBE comprobante, UsersDto User, string CFDIXML, string ViaPago, IEnumerable<ComprobanteBE> notasCredito, IEnumerable<NotaCreditoCFDIXMLDto> notasCreditoXML, string Correo, string comprobanteOriginal, string CFDIVersion);
    }
}
