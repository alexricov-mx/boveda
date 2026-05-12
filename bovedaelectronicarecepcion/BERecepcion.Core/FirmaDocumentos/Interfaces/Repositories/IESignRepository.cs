using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.IntegracionEFirma;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories
{
    public interface IESignRepository
    {
        Task<DataResult<UsuarioDto>> GetValidaOCreaUsuarioAsync(UsuarioDto usuario);
        Task<DataResult<ExternosDto>> PostDocumentoAsync(ExternosDto externosCollection, IFormFile documentoPDF);
        //Task<DataResult<ExternosDto>> PostDocumentoByCorrelationIDAsync(Guid CorrelationID, ExternosDto externos, IFormFile documentoPDF);
        
        Task<DataResult<ExternosDto>> ConsultaPaqueteDocumentoId(int PaqueteId);
        Task<DataResult<ExternosDto>> ConsultaDocumentoFirmaAsync(int PaqueteId, int DocumentoId, int UsuarioId);
        Task<DataResult<ExternosDto>> PostAgregaFirmanteAsync(ExternosDto externosDto);
        Task<DataResult<ExternosDto>> PostFirmaAsync(ExternosDto externosDto, string orden);
        
        Task<DataResult<string>> ConsultaEstadoOCSP(IFormFile cerClientePath);
        Task<DataResult<string>> ValidaCertificado(string CertificadoB64);
        Task<DataResult<ArchivoPDFDto>> RecuperaPDFFirmado(int PaqueteId, int DocumentoId);
        Task<DataResult<ArchivoPDFDto>> RecuperaPDFAPFirmado(int PaqueteId, int DocumentoId);

        // EFirma v2
        Task<DataResult<CrearPaqueteResult>> PostDocumentoEFirmaAsync(string NomArchivo, CrearPaquete paquete, IFormFile documentoPDF);
        Task<DataResult<FirmarPaqueteResult>> PostFirmaEFirmaAsync(FirmarPaquete firmaPaquete);
        Task<DataResult<string>> PostAgregaFirmanteEFirmaAsync(AgregaFirmante agregaFirmante);
        Task<DataResult<CrearPaqueteResult>> GetDocumentoEFirmaAsync(Guid IdCorrelacion);
        Task<DataResult<ArchivoPDFDto>> RecuperaPDFFirmadoEFirmaAsync(Guid IdCorrelacion);
    }
}