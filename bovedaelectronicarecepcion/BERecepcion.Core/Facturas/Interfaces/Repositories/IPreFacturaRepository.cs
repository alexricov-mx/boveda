using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface IPreFacturaRepository
    {
        Task<DataResult<IEnumerable<PreFacturaDto>>> GetPreFacturaAsync(string start, string end, string search, string creditorNumber, Guid userId, int pageSize, int pageNum = 1, bool esDescarga = false);
        Task<DataResult<PreFacturaDto>> GetPreFacturaXmlAsync(Guid CopadeID);
        Task<DataResult<IEnumerable<PreXmlMasDto>>> GetPreFacturaXmlMasAsync(IEnumerable<PreXmlMasDto> dto);
        Task<DataResult<IEnumerable<PreFacturaDto>>> GetPreFacturaBuscarAsync(string creditorNumber, int pageSize, int pageNum = 1, string busqueda = null, DateTime? fechaInicial = null, DateTime? fechaFinal = null);
    }
}
