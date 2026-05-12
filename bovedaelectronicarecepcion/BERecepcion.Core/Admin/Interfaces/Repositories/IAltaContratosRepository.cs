using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IAltaContratosRepository
    {
        Task<DataResult<IEnumerable<AltaContratosDto>>> ConsultaACAsync(int pageSize, int pageNum = 1, string search = null, bool esDescarga = false);
        Task<DataResult<AltaContratosDto>> GetACByIdAsync(Guid AltaContratoID);
        Task<DataResult<AltaContratosDto>> InsertaACAsync(AltaContratosDto altaContratosDto);
        Task<DataResult<AltaContratosDto>> ActualizaACAsync(AltaContratosDto ACDto);
        Task<DataResult<AltaContratosDto>> BorraACAsync(Guid AltaContratoID);

        Task<DataResult<IEnumerable<AltaContratosDto>>> ConsultaACGetAllAsync(int pageSize, int pageNum);
    }
}
