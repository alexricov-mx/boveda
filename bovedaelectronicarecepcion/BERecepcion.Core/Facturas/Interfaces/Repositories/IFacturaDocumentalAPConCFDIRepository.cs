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
    public interface IFacturaDocumentalAPConCFDIRepository
    {
        Task<InvoiceResultDto> SaveInvoice(string Organismo, string IdAnaliticoPago, string RFCReceptor, string DocumentoBEId, ComprobanteBE comprobante, UsersDto User, string CFDIXML, string ViaPago, string ComprobanteOriginal);
        Task<InvoiceResultDto> processSaveInvoice(string Organismo, string IdAnaliticoPago, string RFCReceptor, string DocumentoBEId, ComprobanteBE comprobante, UsersDto User, string CFDIXML, string ViaPago, string Correo, string ComprobanteOriginal);
    }
}
