using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories
{
    public interface ISOEstimationRepository
    {
        Task<IEnumerable<SOEstimationDto>> GetSOEstimacionAsync(string Contract);
        Task<DataResult<IEnumerable<SOEstimationDto>>> GetSOEInternoAsync(string Token, int pageSize, int pageNum = 1);
        Task<DataResult<IEnumerable<SOEstimationDto>>> GetSOEProveedorAsync(string CreditorNumber, int pageSize, int pageNum = 1);
        Task<DataResult<IEnumerable<SOEstimationDto>>> GetSOEstimacionFiltroAsync(string Token, string Filtro);

        Task<DataResult<SOEstimationDto>> EstimationFirmaAsync(Guid EstimacionID, string UserType);
        Task<DataResult<SOEstimationDto>> EstimationFirmaCorreoAsync(Guid EstimacionID, string UserType, string SignerEmail);

        Task<DataResult<SOEstimationDto>> EstimationFirmaNotificacionAsync(Guid EstimacionID, string UserType);

        Task<DataResult<IEnumerable<SOEstimationDto>>> GetListaEstimacionesBancariasAsync(DateTime start, DateTime end, string search, string UserID, string claveOrganismo, string creditorNumber, int pageSize, int pageNum = 1, bool esDescarga = false);
        Task<DataResult<IEnumerable<SOEstimationDto>>> GetESAsync(int pageSize, Guid userId, int pageNum = 1, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null, bool esDescarga = false);
        Task<Guid> GetEstimacionIdAsync(SOEstimationDto dto);
        Task<DataResult<SupplyOrderDto>> EstimacionCorreo1Async(Guid EstimacionID, string SignerEmail);
        Task<DataResult<SupplyOrderDto>> EstimacionCorreo2Async(Guid EstimacionID, string SignerEmail);
        Task<DataResult<IEnumerable<SupplyOrderDto>>> GetListaOrdenesBancariasAsync(DateTime start, DateTime end, string search, string UserID, string claveOrganismo, string creditorNumber, int pageSize, int pageNum = 1, bool esDescarga = false);
    }
}