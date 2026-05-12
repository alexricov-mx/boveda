using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories
{
    public interface IDocumentosRepository
    {
        Task<DataResult<ArchivoPDFDto>> GetDocumentoAsync(string SAPOrder, string Organismo);
        Task<RestResponse> GetDocumentoESignAsync(string paqueteId, string documentoId);
        Task<DataResult<ArchivoPDFDto>> GetDocumentoAPAsync(string itransport, string icte, string ianio, string ilistado_emb);
        Task<ComprobanteDto> GetDocumentoFacturaAsync(string InvoiceId);
        Task<ComprobanteDto> GetDocumentoNotaCreditoAsync(string NotaCreditoId);
        Task<ComprobanteDto> GetDocumentoComplementoPagoAsync(string PagosId);
        Task<PaymentListDto> GetDocumentoListaPagoAsync(string ListaPago_id);
        Task<DataResult<ArchivoPDFDto>> GetDocumentoProgramaPagoAsync(string ProgramaPago_id, string Organismo);
        Task<LogoDto> GetImagenAsync(string clave);
    }
}
