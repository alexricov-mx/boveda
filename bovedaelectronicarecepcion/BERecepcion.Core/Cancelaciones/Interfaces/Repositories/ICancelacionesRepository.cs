using BERecepcion.Core.Cancelaciones.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Cancelaciones.Interfaces.Repositories
{
    public interface ICancelacionesRepository
    {
        Task<DataResult<IEnumerable<CancelacionesCopadeDto>>> GetBusquedaCancelacionesCopadeAsync(string token, Guid userId, DateTime? fechaInicial, DateTime? fechaFinal, string busqueda, int pageSize, int pageNum = 1);
        Task<DataResult<CancelacionesCopadeDto>> CancelacionesCopadeMasiva(CancelacionesCopadeDto dto);

        #region PaymentSchedule

        Task<DataResult<IEnumerable<PaymentScheduleDto>>> GetBusquedaCancelacionesPaymentScheduleAsync(string token, Guid userId, DateTime? fechaInicial, DateTime? fechaFinal, string busqueda, int pageSize, int pageNum = 1);
        Task<DataResult<PaymentScheduleDto>> CancelacionesPaymentScheduleMasivaAsync(PaymentScheduleDto dto);
        #endregion

        #region PaymentList
        Task<DataResult<IEnumerable<PaymentListDto>>> GetBusquedaCancelacionesPaymentListAsync(string token, Guid userId, DateTime? fechaInicial, DateTime? fechaFinal, string busqueda, int pageSize, int pageNum = 1);
        Task<DataResult<PaymentListDto>> CancelacionesPaymentListMasivaAsync(PaymentListDto dto);
        #endregion


    }
}
