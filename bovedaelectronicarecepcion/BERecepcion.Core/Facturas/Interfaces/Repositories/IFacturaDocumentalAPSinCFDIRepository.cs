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
    public interface IFacturaDocumentalAPSinCFDIRepository
    {
        Task<InvoiceResultDto> SaveInvoice(string Organismo, string IdAnaliticoPago, string RFCReceptor, string Correo, InvoiceDto dto, UsersDto User);
        Task<InvoiceResultDto> ValidateInvoice(string Organismo, string IdAnaliticoPago, string RFCReceptor, string Correo, InvoiceDto dto, UsersDto User);
    }
}
