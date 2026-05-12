using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface ICFDIValidationRepository
    {
        Task<List<ValidationError>> validateDetailRFC(string receptorRFC, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, string documento, bool esNotaCredito);
        Task<List<ValidationError>> validateDetailHeader(string clave, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, string documento, bool esNotaCredito);
        Task<List<ValidationError>> validateDetailConcepts(string clave, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, string documento, bool esNotaCredito);
        Task<List<ValidationError>> validateDetailComprobante(string clave, string receptorRFC, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, string documento, bool esNotaCredito, string ComprobanteOriginal);
        Task<List<ValidationError>> validateDetailNCComprobante(string clave, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, bool esNotaCredito, string cfdiEmisorRFC, string cfdiReceptorRFC, string cfdiMoneda, string documento);
        Task<List<ValidationError>> validateDetailHeaderAP(IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, InvoiceValidateDto dto, AnaliticoPagoDto analitico, ComprobanteBE comprobante, string sourceDocument, bool esNotaCredito);
        Task<List<ValidationError>> validateDetailConceptsAP(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, InvoiceValidateDto dto, AnaliticoPagoDto analitico, string sourceDocument, bool esNotaCredito);
        Task<List<ValidationError>> validateDetailComprobateAP(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, InvoiceValidateDto dto, AnaliticoPagoDto analitico, ComprobanteBE comprobante, string sourceDocument, bool esNotaCredito, string ComprobanteOriginal);
        List<ValidationError> addError(bool esNotaCredito, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, string clave, string claveNC, string documento);
    }
}
