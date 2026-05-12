using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface IFacturaPDFRepository
    {
        Task<DataResult<IEnumerable<InvoiceDto>>> GetFacturaPDFAsync(Guid CopadeID);
        Task<DataResult<IEnumerable<InvoiceNotaCreditoDto>>> GetNotaCreditoPDFAsync(Guid InvoiceId);
        Task<DataResult<IEnumerable<CopadeDto>>> GetComprobantePDFAsync(Guid PagoUUID);
    }
}
