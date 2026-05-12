using BERecepcion.Core.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Instrucciones.Interfaces.Repositories
{
    public interface IPaymentListRepository
    {
        Task<DataResult<IEnumerable<PaymentListDto>>> GetPayments(string Token, int pageSize, string search = null, int pageNum = 1);
        Task<DataResult<PaymentListDto>> PaymentSignAsync(Guid PaymentListID);
        Task<DataResult<PaymentListDto>> PaymentMailAsync(Guid PaymentListID, bool? isNotification = null);
    }
}
