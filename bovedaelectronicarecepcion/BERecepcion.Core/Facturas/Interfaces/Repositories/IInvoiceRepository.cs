using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface IInvoiceRepository
    {
        Task<DataResult<InvoiceDto>> GetInvoiceByReception(string claveOrganismo, string reception, string exercise);
        Task<DataResult<InvoiceDto>> GetInvoiceIDByReception(string claveOrganismo, string reception, string exercise);
        Task<DataResult<InvoiceDto>> GetInvoiceByAnaliticoPago(string idAnalitico);
        Task<DataResult<InvoiceDto>> GetInvoiceIDByAnaliticoPago(string idAnalitico);
    }
}
