using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Interfaces.Repositories
{
    public interface IRecepcionEPRepository
    {
        Task<DataResult<bool>> GetValidaInvoiceDocAsync(Guid uuid);
        Task<DataResult<RecepcionElectronicaPDto>> InsertaPagosDocAsync(RecepcionElectronicaPDto dto);
    }
}
