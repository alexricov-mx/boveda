using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Consulta.Interfaces.Repositories
{
    public interface IProveedoresEmailRepository
    {
        Task<DataResult<IEnumerable<CopadeDto>>> GetProveedoresEmailGAsync(DateTime fechaInicial, DateTime fechaFinal, string search, Guid UserID, int pageSize, int pageNum = 1, bool esDescarga = false);
    }
}
