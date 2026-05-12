using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IBitacoraRepository
    {
        Task<DataResult<IEnumerable<BitacoraDto>>> GetBitacoraAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda, int pageSize, int pageNum);
        Task<DataResult<IEnumerable<BitacoraDto>>> GetAllBitacoraAsync(DateTime fechaInicial, DateTime fechaFinal, string busqueda);
        Task<DataResult<BitacoraDto>> InsertaBitacoraAsync(BitacoraDto dto);
        Task<DataResult<BitacoraInvoiceDto>> InsertaBitacoraInvoiceAsync(BitacoraInvoiceDto dto);
        Task<DataResult<BitacoraInvoiceErrorInsertDto>> InsertaBitacoraInvoiceErrorAsync(BitacoraInvoiceErrorInsertDto dtoList);
        Task<bool> BitacoraInvoice(string reception, string clave, UsersDto user, bool process);
        Task<bool> BitacoraInvoiceError(BitacoraInvoiceErrorInsertDto dto);
        Task<bool> InvoiceSentMail(bool sentMail, string reception, string Email);
        Task<bool> logInitialRequest(string claveOrganismo, string reception, string user, string xml, List<string> ncXml, bool esCopade, bool esDocumental);
        Task<bool> BitacoraInvoiceAP(string IdAnaliticoPago, Guid documentBeId, string clave, UsersDto user, bool process);
        Task<bool> InvoiceSentMailByDocumentoBEId(bool sentMail, Guid documentoBEId, string Email);
        Task<DataResult<IEnumerable<ReporteRechazosDto>>> GetListaRechazosAsync(DateTime start, DateTime end, string search, Guid userId, int source, int pageSize, int pageNum = 1, bool esDescarga = false);
    }
}
