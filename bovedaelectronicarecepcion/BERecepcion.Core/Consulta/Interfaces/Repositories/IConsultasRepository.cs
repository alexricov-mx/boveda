using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Consulta.Interfaces.Repositories
{
    public interface IConsultasRepository
    {
        Task<DataResult<IEnumerable<ReportePaymentScheduleDto>>> GetListaPaymentScheduleAsync(DateTime start, DateTime end, string filtro, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false);
        Task<DataResult<IEnumerable<ReportePaymentListDto>>> GetListaPaymentListAsync(DateTime start, DateTime end, string filtro, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false);
        Task<DataResult<IEnumerable<EstadoFacturaDto>>> GetEstadoFacturasAsync(DateTime start, DateTime end, string search, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false);
        Task<PagedResult<SOEstimationDto>> GetEstimacionesObraAsyncRefactorAsync(
            EstimacionesObraRequest request, 
            CancellationToken cancellationToken = default);

        Task<PagedResult<SupplyOrderDto>> GetOrdenesSurtimientoAsync(
            OrdenSurtimientoRequest request, 
            CancellationToken cancellationToken = default);
    }
}
