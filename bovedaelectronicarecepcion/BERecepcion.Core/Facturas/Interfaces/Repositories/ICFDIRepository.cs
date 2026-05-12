using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.SAT.Dto;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface ICFDIRepository
    {
        Task<DataResult<IEnumerable<ValidationError>>> GetValidationErrorCatalog();
        Task<string> GetOrganismRFC(string organismId);
        Task<List<string>> GetValidVersions();
        List<ValidationError> addError(bool esNotaCredito, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, string clave, string claveNC, string documento);
        public List<int> setCopadeNC(string copadeNotasCredito, ComprobanteBE notaCredito, out CopadeDto copade, List<int> processed);
        InvoiceDto setInvoiceDto(ComprobanteDto comprobante, CopadeDto copade, string tipoComprobante, bool isElectronicReception, bool isCopade, Guid UserId);
        CXPDto setApiCXPDto(ComprobanteDto comprobante, CopadeDto copade, string tipoComprobante, bool isElectronicReception, bool isCopade, string token);
        InvoiceNotaCreditoDto setInvoiceNotaCreditoDto(ComprobanteBE comprobante, Guid DocumentoBEId, Guid InvoiceId, bool isCopade, double amount, double iva, double total);
        DateTime convertToDate(string d);
        Task<ComprobanteDto> SaveInvoice(ComprobanteDto comprobante);
        Task<IEnumerable<ComprobanteDto>> SaveInvoiceMultiple(IEnumerable<ComprobanteDto> comprobantes);
        Task<InvoiceProcessDto> processInvoice(ComprobanteDto comprobante, CopadeDto copade, IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, string tipoComprobante, UsersDto User, string documento);
        Task<List<ValidationError>> ValidatePep(string claveOrganismo, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, string documento, bool esNotaCredito);
        Task<List<ValidationError>> ValidatePepAP(string claveOrganismo, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, AnaliticoPagoDto analitico, string documento, bool esNotaCredito);
        Task<ComprobanteDto> processSaveInvoice(ComprobanteDto comprobante);
        Task<bool> logInitialRequest(ComprobanteDto comprobante);
        Task<List<ValidationError>> validateDetailComprobante(string clave, string receptorRFC, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, string documento, bool esNotaCredito, string comprobanteOriginal);
        Task<List<ValidationError>> validateDetailNCComprobante(string clave, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, bool esNotaCredito, string cfdiEmisorRFC, string cfdiReceptorRFC, string cfdiMoneda, string documento);
        Task<List<ValidationError>> validateDetailComprobateAP(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, InvoiceValidateDto dto, AnaliticoPagoDto analitico, ComprobanteBE comprobante, string sourceDocument, bool esNotaCredito, string comprobanteOriginal);
        Task<bool> setInitialRequestRejected(string reception, string correo, string RFCEmisor, string RFCReception);
        Task<DataResult<List<PendingInvoiceDto>>> GetPendingInvoice();
        Task<DataResult<bool>> ReprocessInvoice();
        Task<DataResult<InvoiceValidateDto>> ValidateInvoiceAP(DataResult<InvoiceValidateDto> dto, string idAnalitico, string sourceDocument, ComprobanteBE comprobante, string comprobanteOriginal);
        string cxpError(string uuid);
        CXPResultDto setCxPResult(System.Net.HttpStatusCode Status, CXPDto Data, string uuid, IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, string documento, bool existsCriticalError);
        Task<SATResultDto> setSATResult(string RFCEmisor, string RFCReceptor, string Total, Guid UUID, List<ValidationError> validationErrors, IEnumerable<ValidationError> errorCatalog, bool existsCriticalError, string sourceDocument);
    }
}
