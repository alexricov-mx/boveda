using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Models;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories
{
    public interface ISupplyOrderRepository
    {
        Task<DataResult<IEnumerable<SupplyOrderDto>>> GetOSInternoAsync(string Token, int pageSize, string search = null, int pageNum = 1);
        Task<DataResult<IEnumerable<SupplyOrderDto>>> GetOSProveedorAsync(string CreditorNumber, int pageSize, string search = null, int pageNum = 1);
        Task<DataResult<SupplyOrderDto>> SupplyOrderFirmaAsync(Guid SupplyOrderID, string UserType, string Ficha);
        Task<DataResult<SupplyOrderDto>> SupplyOrderFirmaCorreoAsync(Guid SupplyOrderID, string UserType, string SignerEmail);
        Task<DataResult<SupplyOrderDto>> SupplyOrderFirmaNotificacionAsync(Guid SupplyOrderID, string UserType);
        Task<DataResult<IEnumerable<SupplyOrderDto>>> GetOSAsync(int pageSize, Guid userId, int pageNum = 1, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null, bool esDescarga = false);
        Task<Guid> GetOSSupplyOrderIdAsync(SupplyOrderDto dto);
        Task<DataResult<SupplyOrderDto>> SupplyOrderCorreo1Async(Guid SupplyOrderID, string SignerEmail);
        Task<DataResult<SupplyOrderDto>> SupplyOrderCorreo2Async(Guid SupplyOrderID, string SignerEmail);
        Task<PagedResult<SupplyOrderDto>> GetListPaginatedSupplyOrderAsync(
            string token,
            int pageSize,
            int pageNumber,
            string search,
            CancellationToken cancellationToken
            );
        Task<PagedResult<SupplyOrderDto>> GetListPaginatedSupplyOrderByProveedorAsync(
            string creditorNumber,
            int pageSize,
            int pageNumber,
            string search,
            CancellationToken cancellationToken
            );


    }
}