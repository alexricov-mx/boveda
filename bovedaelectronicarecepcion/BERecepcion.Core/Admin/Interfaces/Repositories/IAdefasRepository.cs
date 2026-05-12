using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IAdefasRepository
    {
        Task<DataResult<AdefasDto>> InsertaAdefaAsync(AdefasDto adefaDto);
        Task<DataResult<AdefasDto>> ActualizaAdefaAsync(AdefasDto adefaDto);
        Task<DataResult<IEnumerable<AdefasDto>>> GetAdefasAsync(int pageSize, int pageNum = 1, string search = null, bool esDescarga = false);
        Task<DataResult<AdefasDto>> BorraAdefaAsync(Guid AdefaID);
        Task<DataResult<ValidaPeriodoAdefaDto>> GetValidaPeriodoAdefaAsync(string clave, string AnhiFactura, string FechaFactura);
        Task<DataResult<AdefasDto>> GetAdefaByIdAsync(Guid AdefaID);
    }
}
