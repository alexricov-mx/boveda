using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface IInvoiceAPCxPRepository
    {
        Task<DataResult<IEnumerable<InvoiceAPCxPList>>> GetInvoiceAPCxPAsync(int pageSize, int pageNum = 1);
        Task<DataResult<IEnumerable<InvoiceAPCxPList>>> GetInvoiceAPCxPFiltroAsync(string Filtro, int pageSize, int pageNum = 1);
    }
}
