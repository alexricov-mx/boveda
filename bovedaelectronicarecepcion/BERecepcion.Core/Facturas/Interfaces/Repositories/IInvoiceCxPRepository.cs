using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface IInvoiceCxPRepository
    {
        Task<DataResult<IEnumerable<InvoiceCxPList>>> GetInvoiceCxPAsync(int pageSize, int pageNum = 1);
        Task<DataResult<IEnumerable<CXPDto>>> GetInvoiceCxPAPAsync();
        Task<DataResult<IEnumerable<InvoiceCxPList>>> GetInvoiceCxPFiltroAsync(string Filtro, int pageSize, int pageNum = 1);
    }
}
