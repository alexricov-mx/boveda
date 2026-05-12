using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface IPrefacturaAPRepository
    {
        Task<DataResult<IEnumerable<PrefacturaAPDto>>> GetPreFacturaAsync(string start, string end, string search, string creditorNumber, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false);
        Task<DataResult<PrefacturaAPDto>> GetPreFacturaXmlAPAsync(Guid AnaliticoPagoID);
        Task<DataResult<IEnumerable<PrefacturaAPDto>>> GetPreFacturaXmlAPMasAsync(IEnumerable<PrefacturaAPDto> dto);
    }
}
