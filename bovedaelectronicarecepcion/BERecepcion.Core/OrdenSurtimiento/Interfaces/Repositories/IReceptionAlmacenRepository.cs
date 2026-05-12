using BERecepcion.Core.Dto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories
{
    public interface IReceptionAlmacenRepository
    {
        Task<DataResult<IEnumerable<ReceptionDto>>> GetReceptionAsync(string Token, int pageSize, int pageNum = 1, string search = null);
        Task<ReceptionDto> RecuperaReceptionAsync(Guid ReceptionId);
        Task<DataResult<ReceptionDto>> ReceptionFirmaAsync(Guid ReceptionId);
    }
}
