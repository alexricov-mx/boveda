using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IBitacoraAdmonRepository
    {
        Task<DataResult<IEnumerable<BitacoraAdmonDto>>> GetBitacoraAdmonAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda, int pageSize, int pageNum);
        Task<DataResult<IEnumerable<BitacoraAdmonDto>>> GetAllBitacoraAdmonAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda);
        Task<DataResult<BitacoraAdmonDto>> InsertaBitacoraAdmonAsync(BitacoraAdmonDto dto);
    }
}
