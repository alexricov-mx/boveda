using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Copades.Interfaces.Repositories
{
    public interface IAnaliticoPagoRepository
    {
        Task<DataResult<IEnumerable<AnaliticoPagoDto>>> ConsultaAPGetAllAsync(int pageSize, string ficha, int pageNum = 1, string search = null);
        Task<DataResult<IEnumerable<UsersDto>>> GetEmailAP(Guid AnaliticoPagoID);
        Task<DataResult<AnaliticoPagoDto>> GetAPByIdAnaliticoAsync(string idAnalitico);
        Task<DataResult<AnaliticoPagoDto>> GetAPByIdAnaliticoClaveAsync(string idAnalitico, string clave);
        Task<DataResult<AnaliticoPagoDto>> APFirmaAsync(Guid AnaliticoPagoID, string Token);
        Task<DataResult<AnaliticoPagoDto>> APFirmaCorreoAsync(Guid AnaliticoPagoID, IEnumerable<UsersDto> users);
        Task<DataResult<IEnumerable<AnaliticoPagoDto>>> GetAPAsync(int pageSize, string ficha, int pageNum = 1, DateTime? fechaInicial = null, DateTime? fechaFinal = null, string search = null, bool esDescarga = false);
        Task<ComprobanteDto> GetXMLAPAsync(Guid InvoiceId);
        Task<ComprobanteDto> GetXMLAPAsync2(Guid NotaCreditoId);
        Task<ComprobanteDto> GetXMLAPAsync3(Guid PagosID);
    }
}
