using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IDesvioFirmasRepository
    {
        //Task<DataResult<IEnumerable<DesvioFirmasDto>>> GetDesvioAsync(int contrato);
        Task<DataResult<DesvioPendientesDto>> GetDesvioPendientesAsync(string contrato);        
        Task<DataResult<List<RealizaDesvioResponseDto>>> PostRealizaDesvioFAsync(RealizaDesvioFirmasDto desvio);
    }
}
