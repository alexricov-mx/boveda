using BERecepcion.Core.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Instrucciones.Interfaces.Repositories
{
    public interface IPaymentScheduleRepository
    {
        Task<DataResult<IEnumerable<PaymentScheduleDto>>> GetPayments(string Token, int pageSize, string search = null, int pageNum = 1);
        Task<DataResult<PaymentScheduleDto>> PaymentSignAsync(Guid PaymentScheduleID);
        Task<DataResult<PaymentScheduleDto>> PaymentMailAsync(Guid PaymentScheduleID, bool? isNotification = null);
    }
}