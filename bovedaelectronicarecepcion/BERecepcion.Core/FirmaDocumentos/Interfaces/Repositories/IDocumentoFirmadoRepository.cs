using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories
{
    public interface IDocumentoFirmadoRepository
    {
        Task<DataResult<DocumentoFirmadoDto>> GetDocumentoFirmadoAsync(Guid DocumentoBEId, int? Orden = null);
        Task<string> CreaPaqueteAsync(ExternosDto externos, string orden);
        Task<Guid> CreaPaqueteInicialAsync(string usuarioId, string DocumentType, Guid usuarioBEId, Guid documentoBEId, string orden);
        Task ActualizaPaqueteInicialAsync(Guid correlationId, string paqueteId, string documentoId, string orden);
        Task<string> ActualizaFirmaAsync(ExternosDto externos, string orden);
        Task<string> ActualizaFirmaByCorrelationIdAsync(Guid correlationId);
        Task<DataResult<DocumentoFirmadoDto>> GetDocumentoAPFirmadoAsync(Guid DocumentoBEId);
    }
}
