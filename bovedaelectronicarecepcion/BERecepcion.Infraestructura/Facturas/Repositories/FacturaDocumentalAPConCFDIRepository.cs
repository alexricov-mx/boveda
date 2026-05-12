using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Xml.Serialization;
using System.Xml;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using BERecepcion.Core.SAT.Dto;
using BERecepcion.Core.SAT.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class FacturaDocumentalAPConCFDIRepository : BaseSQLServerSqlRepository, IFacturaDocumentalAPConCFDIRepository
    {

        private readonly IConfiguration _configuration;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly ISAPPIRepository _sapPIRepository;
        private readonly IFacturaElectronicaRepository _facturaElectronicaRepository;
        private readonly IAdefasRepository _adefasRepository;
        private readonly ISATRepository _sATRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICFDIRepository _cFDIRepository;
        private readonly IAnaliticoPagoRepository _analiticoPagoRepository;
        private readonly ICorreoRepository _correoRepository;
        private readonly ICFDIValidationCartaPorteRepository _cartaPorteRepository;
        private bool processBitacora = false;

        public FacturaDocumentalAPConCFDIRepository(string cnnString,
            IConfiguration configuration,
            IBitacoraRepository bitacoraRepository,
            ISAPPIRepository sapPIRepository,
            IFacturaElectronicaRepository facturaElectronicaRepository,
            IAdefasRepository adefasRepository,
            ISATRepository sATRepository,
            IInvoiceRepository invoiceRepository,
            ICFDIRepository cFDIRepository,
            IAnaliticoPagoRepository analiticoPagoRepository,
            ICorreoRepository correoRepository,
            ICFDIValidationCartaPorteRepository cartaPorteRepository) : base(cnnString)
        {
            _configuration = configuration;
            _bitacoraRepository = bitacoraRepository;
            _sapPIRepository = sapPIRepository;
            _facturaElectronicaRepository = facturaElectronicaRepository;
            _adefasRepository = adefasRepository;
            _sapPIRepository = sapPIRepository;
            _sATRepository = sATRepository;
            _invoiceRepository = invoiceRepository;
            _cFDIRepository = cFDIRepository;
            _analiticoPagoRepository = analiticoPagoRepository;
            _correoRepository = correoRepository;
            _cartaPorteRepository = cartaPorteRepository;
        }

        public async Task<InvoiceResultDto> SaveInvoice(string Organismo, string IdAnaliticoPago, string RFCReceptor, string DocumentoBEId, ComprobanteBE comprobante, UsersDto User, string CFDIXML, string ViaPago, string ComprobanteOriginal)
        {
            var result = await processSaveInvoice(Organismo, IdAnaliticoPago, RFCReceptor, DocumentoBEId, comprobante, User, CFDIXML, ViaPago, User.Email, ComprobanteOriginal);

            List<ValidationError> validationErrors = result.validationErrors.ToList();

            try
            {
                bool processBitacora = true;
                try
                {
                    processBitacora = Convert.ToBoolean(_configuration["BitacoraInvoice:Habilitado"]);
                }
                catch (Exception exx)
                {
                    processBitacora = true;
                    Log.Error(exx.Message);
                }
                var validacionError = await _correoRepository.NotificacionFacturaAPEmailAsync(IdAnaliticoPago, validationErrors, User, Guid.Parse(DocumentoBEId), User.Email, processBitacora, "FacturaDocumentalAPConCFDIRepository", "SaveInvoice");
            }
            catch (Exception ex)
            {
                var err = new ValidationError() { clave = "70000", descripcion = ex.Message, documento = "" };
                validationErrors.Add(err);
                result.validationErrors = validationErrors;
                Log.Error(ex.Message);
            }

            return result;
        }

        public async Task<InvoiceResultDto> processSaveInvoice(string Organismo, string IdAnaliticoPago, string RFCReceptor, string DocumentoBEId, ComprobanteBE comprobante, UsersDto User, string CFDIXML, string ViaPago, string Correo, string ComprobanteOriginal)
        {
            bool allowDuplicated = false;
            bool validateSAT = true;
            bool validateVersion = true;
            AnaliticoPagoDto analitico = null;
            bool existsCriticalError = false;
            string sapError = "";
            Guid invoiceIdResult = Guid.Empty;
            string exceptionMessage = "";
            bool existedException = false;
            int status = -1;

            InvoiceDto dto = setInvoiceDto(comprobante, DocumentoBEId, "I", true, false, User.UserID, ViaPago);

            dto.UserId = User.UserID;
            dto.TipoComprobante = "I";
            dto.ViaPago = ViaPago;

            InvoiceResultDto invoiceResultDto = new InvoiceResultDto();
            invoiceResultDto.Status = -1;
            invoiceResultDto.validationErrors = null;
            invoiceResultDto.ExceptionMessage = "";
            invoiceResultDto.ExistedException = false;
            invoiceResultDto.InvoiceId = Guid.Empty;

            try
            {
                processBitacora = Convert.ToBoolean(_configuration["BitacoraInvoice:Habilitado"]);
                allowDuplicated = Convert.ToBoolean(_configuration["InvoiceValidation:PermitirDuplicados"]);
                validateSAT = Convert.ToBoolean(_configuration["InvoiceValidation:ValidarSAT"]);
                validateVersion = Convert.ToBoolean(_configuration["InvoiceValidation:ValidarVersion"]);
            }
            catch (Exception exx)
            {
                processBitacora = true;
                allowDuplicated = false;
                validateSAT = true;
                validateVersion = false;
                Log.Error(exx.Message);
            }

            List<ValidationError> validationErrors = new List<ValidationError>();

            try
            {
                string xmlEncoded = CFDIXML;

                await _bitacoraRepository.logInitialRequest(Organismo, IdAnaliticoPago, User.UserID.ToString(), xmlEncoded, new List<string>() { }, false, false);

                IEnumerable<ValidationError> errorCatalog = new List<ValidationError>() { };

                await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "60", User, processBitacora);
                var ec = await _cFDIRepository.GetValidationErrorCatalog();
                if (ec != null)
                {
                    if (ec.Data != null)
                    {
                        errorCatalog = ec.Data;
                    }
                    else
                    {
                        existsCriticalError = true;
                    }
                }
                else
                {
                    existsCriticalError = true;
                }

                await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "70", User, processBitacora);

                await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "140", User, processBitacora);

                if (!allowDuplicated)
                {
                    // verificamos que no exista una factura del analitico
                    await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "160", User, processBitacora);
                    var _analiticopagoInvoiceExists = _invoiceRepository.GetInvoiceByAnaliticoPago(IdAnaliticoPago).Result;
                    if (_analiticopagoInvoiceExists.Status != HttpStatusCode.OK)
                    {
                        validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20422", "20422", IdAnaliticoPago);
                        validationErrors[validationErrors.Count - 1].mensaje = _analiticopagoInvoiceExists.Message;
                        existsCriticalError = true;
                    }
                    await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "161", User, processBitacora);
                }

                //carga del analitico de pago
                await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "162", User, processBitacora);
                var analiticoResult = await _analiticoPagoRepository.GetAPByIdAnaliticoAsync(IdAnaliticoPago);
                if (analiticoResult.Status != HttpStatusCode.OK)
                {
                    validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12100", "12100", IdAnaliticoPago);
                    validationErrors[validationErrors.Count - 1].mensaje = analiticoResult.Message;
                    existsCriticalError = true;
                }
                else
                {
                    analitico = analiticoResult.Data;
                }
                await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "163", User, processBitacora);

                //validacion del invoice
                DataResult<InvoiceValidateDto> idto = new DataResult<InvoiceValidateDto>();
                idto.User = User;
                idto.Data = setInvoiceValidateDto(dto, comprobante, Organismo, IdAnaliticoPago, validationErrors);
                var validationInvoice = await _cFDIRepository.ValidateInvoiceAP(idto, IdAnaliticoPago, IdAnaliticoPago, comprobante, ComprobanteOriginal);
                var valInvoice = validationInvoice.Data;
                if (valInvoice != null)
                {
                    if (valInvoice.validationErrors != null)
                    {
                        foreach (var iv in valInvoice.validationErrors)
                        {
                            validationErrors.Add(iv);
                            if (iv.esTerminal)
                            {
                                existsCriticalError = true;
                            }
                        }
                    }
                }

                dto.DiferencialCargo = Convert.ToDouble(dto.Total) < Convert.ToDouble(analitico.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(analitico.Total))), 2).ToString() : "0";
                dto.DiferencialAbono = Convert.ToDouble(dto.Total) > Convert.ToDouble(analitico.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(analitico.Total))), 2).ToString() : "0";
                dto.ImporteOriginal = analitico.Total;
                dto.Subtotal = analitico.SubTotal;
                dto.TotalImpuestosTrasladados = analitico.vImpuestos.totalImpuestosTrasladados;
                dto.TotalImpuestosRetenidos = analitico.vImpuestos.totalImpuestosRetenidos;

                //validacion del SAT
                if (validateSAT)
                {
                    await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "420", User, processBitacora);
                    SATResultDto sATResultDto = await _cFDIRepository.setSATResult(User.CreditorRFC, RFCReceptor, dto.Total, dto.Uuid, validationErrors, errorCatalog, existsCriticalError, IdAnaliticoPago);
                    existsCriticalError = sATResultDto.existsCriticalError;
                    validationErrors = sATResultDto.validationErrors;
                    await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "430", User, processBitacora);
                }

                await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "150", User, processBitacora);

                //validacion de Adefa
                //debe existir una preconfiguracion en Adefas para procesarlo
                string yearDocument = dto.InvoiceDate.Year.ToString();
                string yearCurrent = DateTime.Now.Year.ToString();
                var resultRecuperado = await _adefasRepository.GetValidaPeriodoAdefaAsync(Organismo, analitico.Ejercicio, dto.InvoiceDate.ToString("yyyy-MM-dd"));
                if (resultRecuperado.Status == HttpStatusCode.OK)
                {
                    bool r = false;
                    if (resultRecuperado.Data != null)
                    {
                        r = resultRecuperado.Data.valido;
                    }
                    if (r == false)
                    {
                        validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20620", "20620", IdAnaliticoPago);
                        existsCriticalError = true;
                    }
                }

                //validacion CxP
                //llamada a la api
                await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "180", User, processBitacora);
                CXPDto cXP = setApiCXPDto(Organismo, analitico, dto, User);
                var respApiCXP = await _sapPIRepository.PostCxP(cXP);
                await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "190", User, processBitacora);
                if (!existsCriticalError)
                {
                    string uuid = dto.Uuid.ToString();
                    CXPResultDto cXPResultDto = _cFDIRepository.setCxPResult(respApiCXP.Status, respApiCXP.Data, uuid, errorCatalog, validationErrors, IdAnaliticoPago, existsCriticalError);
                    dto.SapDocument = cXPResultDto.sapDocument;
                    sapError = cXPResultDto.sapError;
                    validationErrors = cXPResultDto.validationErrors;
                    existsCriticalError = cXPResultDto.existsCriticalError;
                }

                if(validateVersion)
                {
                    List<string> validVersions = await _cFDIRepository.GetValidVersions();
                    if (validVersions.Count > 0)
                    {
                        var x = validVersions.Where(x => x == comprobante.Version).ToList();
                        if (x == null || x.Count == 0)
                        {
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20900", "20900", IdAnaliticoPago);
                            existsCriticalError = true;
                        }
                    }
                }

                status = 0;

                if (!existsCriticalError)
                {
                    //alta de cxp
                    InvoiceCxPDto iCxp = new InvoiceCxPDto();
                    iCxp.CxPId = Guid.Empty;
                    iCxp.InvoiceId = Guid.Empty;
                    if (respApiCXP.Data == null)
                    {
                        iCxp.Res = null;
                    }
                    else
                    {
                        iCxp.Res = respApiCXP.Data.Mensaje;
                    }
                    iCxp.UserId = User.UserID;

                    //notas de credito
                    List<InvoiceNotaCreditoDto> lrNC = new List<InvoiceNotaCreditoDto>() { };
                    List<string> lrNCXML = new List<string>() { };

                    await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "160", User, processBitacora);
                    InvoiceSaveGralDto invoiceSaveGralDto = new InvoiceSaveGralDto();
                    invoiceSaveGralDto.invoiceDto = dto;
                    invoiceSaveGralDto.invoiceXML = xmlEncoded;
                    invoiceSaveGralDto.invoiceCxPDto = iCxp;
                    invoiceSaveGralDto.invoiceNotasCreditoDto = lrNC;
                    invoiceSaveGralDto.InvoiceNotasCreditoXML = lrNCXML;
                    var respAdd = await _facturaElectronicaRepository.SaveInvoiceGral(invoiceSaveGralDto);
                    await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "170", User, processBitacora);
                    if (respAdd.Status != HttpStatusCode.OK || respAdd.Data == null)
                    {
                        validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "60003", "60003", IdAnaliticoPago);
                    }
                    else
                    {
                        invoiceIdResult = respAdd.Data.invoiceDto.InvoiceId;
                        status = 1;
                    }
                }

                if (sapError != "")
                {
                    await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, sapError, User, processBitacora);
                }
                await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, dto.DocumentoBEId, "330", User, processBitacora);

                if (!string.IsNullOrEmpty(IdAnaliticoPago))
                {
                    if (validationErrors.Count > 0)
                    {
                        BitacoraInvoiceErrorInsertDto bie = new BitacoraInvoiceErrorInsertDto();
                        bie.reception = IdAnaliticoPago;
                        bie.medioProceso = 0;
                        bie.UserId = User.UserID.ToString();
                        bie.errors = validationErrors;

                        var respbie = await _bitacoraRepository.InsertaBitacoraInvoiceErrorAsync(bie);
                    }
                }

                invoiceResultDto.validationErrors = validationErrors;
                invoiceResultDto.InvoiceId = invoiceIdResult;
                int qtyCritical = validationErrors.Where(x => x.esTerminal == true).Count();
                if (qtyCritical > 0)
                {
                    var v = validationErrors.Where(x => x.clave == "60000" || x.clave == "60001").FirstOrDefault();
                    if (v == null)
                    {
                        status = -1;
                        await _cFDIRepository.setInitialRequestRejected(IdAnaliticoPago, Correo, analitico.CreditorRFC, RFCReceptor);
                    }
                }

                invoiceResultDto.Status = status;

            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
                existedException = true;
                Log.Error(exceptionMessage);

                try
                {
                    if (validationErrors.Count > 0)
                    {
                        BitacoraInvoiceErrorInsertDto biee = new BitacoraInvoiceErrorInsertDto();
                        biee.reception = IdAnaliticoPago;
                        biee.medioProceso = 0;
                        biee.UserId = User.UserID.ToString();
                        biee.errors = validationErrors;

                        var respbie = await _bitacoraRepository.InsertaBitacoraInvoiceErrorAsync(biee);
                    }
                }
                catch (Exception exx)
                {
                    Log.Error(exx.Message);
                }
            }

            invoiceResultDto.ExistedException = existedException;
            invoiceResultDto.ExceptionMessage = exceptionMessage;
            invoiceResultDto.InvoiceId = invoiceIdResult;

            return invoiceResultDto;
        }

        private CXPDto setApiCXPDto(string Organismo, AnaliticoPagoDto analitico, InvoiceDto dto, UsersDto User)
        {
            CXPDto r = new CXPDto()
            {
                Organismo = "SIIC",
                Ejercicio = analitico.Ejercicio,
                FechaRecep = DateTime.Now.ToString("yyyy-MM-dd"),
                FechaFactura = dto.InvoiceDate.ToString("yyyy-MM-dd"),
                Factura = dto.Uuid == Guid.Empty ? string.Concat(dto.Serie, " ", dto.Folio) : dto.Uuid.ToString(),
                ViaPago = dto.Assignment,
                Usuario = User.Token,
                ContratoVigente = analitico.Contrato,
                Cliente = analitico.NumCliente,
                Id_Analitico = analitico.IdAnalitico,
                CentroGestor = analitico.Centro,
                ImporteFactura = dto.Total,
                ImporteOriginal = analitico.Total,
                DiferencialCargo = Convert.ToDouble(dto.Total) < Convert.ToDouble(analitico.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(analitico.Total))), 2).ToString() : "0",     // pendiente la regla de diferencial cargo/abono
                DiferencialAbono = Convert.ToDouble(dto.Total) > Convert.ToDouble(analitico.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(analitico.Total))), 2).ToString() : "0"
            };

            return r;
        }

        private InvoiceAP setInvoiceForXmlDto(InvoiceDto dto)
        {
            InvoiceAP xDto = new InvoiceAP();
            xDto.Assignment = dto.Assignment;
            xDto.OriginalXML = dto.OriginalXML;
            xDto.CompensationDocument = dto.CompensationDocument;
            xDto.DiferencialAbono = dto.DiferencialAbono;
            xDto.DiferencialCargo = dto.DiferencialCargo;
            xDto.DocumentoBEId = dto.DocumentoBEId;
            xDto.ElectronicReception = dto.ElectronicReception;
            xDto.EmailProvider = dto.EmailProvider;
            xDto.EmailProviderSendDate = dto.EmailProviderSendDate;
            xDto.Estatus = dto.Estatus;
            xDto.Folio = dto.Folio;
            xDto.ImporteOriginal = dto.ImporteOriginal;
            xDto.InvoiceDate = dto.InvoiceDate;
            xDto.InvoiceId = dto.InvoiceId;
            xDto.IsCopade = dto.IsCopade;
            xDto.LastStatus = dto.LastStatus;
            xDto.LastStatusDate = dto.LastStatusDate;
            xDto.ReceptionDate = dto.ReceptionDate;
            xDto.RutaArchivo = dto.RutaArchivo;
            xDto.SapDocument = dto.SapDocument;
            xDto.Serie = dto.Serie;
            xDto.Subtotal = dto.Subtotal;
            xDto.TipoComprobante = dto.TipoComprobante;
            xDto.Total = dto.Total;
            xDto.TotalImpuestosRetenidos = dto.TotalImpuestosRetenidos;
            xDto.TotalImpuestosTrasladados = dto.TotalImpuestosTrasladados;
            xDto.UserId = dto.UserId;
            xDto.Uuid = dto.Uuid;
            xDto.ViaPago = dto.ViaPago;

            return xDto;
        }

        public InvoiceDto setInvoiceDto(ComprobanteBE comprobante, string DocumentoBEId, string tipoComprobante, bool isElectronicReception, bool isCopade, Guid UserId, string ViaPago)
        {
            InvoiceDto dto = new InvoiceDto();

            double diferencialAbono, diferencialCargo;
            diferencialAbono = diferencialCargo = 0;
            dto.UserId = UserId;
            dto.Assignment = ViaPago;
            dto.OriginalXML = comprobante.Cfdi;
            dto.CompensationDocument = "";
            dto.DiferencialAbono = diferencialAbono.ToString();
            dto.DiferencialCargo = diferencialCargo.ToString();
            dto.DocumentoBEId = Guid.Parse(DocumentoBEId);
            dto.ElectronicReception = isElectronicReception ? "E" : "D";
            dto.EmailProvider = "";
            dto.EmailProviderSendDate = DateTime.Now;
            dto.Estatus = "0";
            dto.FechaEmision = null;
            dto.Folio = comprobante.Folio;
            dto.ImporteOriginal = "0";
            dto.InvoiceDate = convertToDate(comprobante.Fecha);
            dto.InvoiceId = Guid.Empty;
            dto.IsCopade = isCopade;
            dto.LastStatus = "0";
            dto.LastStatusDate = DateTime.Now;
            dto.ReceptionDate = DateTime.Now;
            dto.RutaArchivo = "";
            dto.SapDocument = "";
            dto.Serie = comprobante.Serie;
            dto.Subtotal = comprobante.SubTotal.ToString();
            dto.TipoComprobante = tipoComprobante;
            dto.Total = comprobante.Total.ToString();
            dto.TotalImpuestosRetenidos = comprobante.Impuestos.TotalImpuestosRetenidos.ToString();
            dto.TotalImpuestosTrasladados = comprobante.Impuestos.TotalImpuestosTrasladados.ToString();
            dto.Uuid = comprobante.Complemento.TimbreFiscalDigital.UUID;
            dto.ViaPago = ViaPago;
            dto.CartaPorte = comprobante.Complemento.CartaPorte;
            dto.CFDIVersion = comprobante.Version;

            return dto;
        }

        public DateTime convertToDate(string d)
        {
            if (string.IsNullOrEmpty(d))
            {
                return DateTime.MinValue;
            }
            else
            {
                int yy, mm, dd, h, m, s;
                h = m = s = 0;
                yy = Convert.ToInt32(d.Substring(0, 4));
                mm = Convert.ToInt32(d.Substring(5, 2));
                dd = Convert.ToInt32(d.Substring(8, 2));
                try
                {
                    if (d.Length > 10)
                    {
                        h = Convert.ToInt32(d.Substring(11, 2));
                        m = Convert.ToInt32(d.Substring(14, 2));
                        s = Convert.ToInt32(d.Substring(17, 2));
                    }
                }
                catch (Exception ex)
                {
                    h = m = s = 0;
                    Log.Error(ex.Message);
                }

                return new DateTime(yy, mm, dd, h, m, s);
            }
        }

        private InvoiceValidateDto setInvoiceValidateDto(InvoiceDto dto, ComprobanteBE comprobante, string Organismo, string recepcion, List<ValidationError> validationErrors)
        {
            InvoiceValidateDto i = new InvoiceValidateDto();
            i.claveOrganismo = Organismo;
            i.invoiceDto = dto;
            i.recepcion = recepcion;
            i.validationErrors = validationErrors;
            i.conceptos = comprobante.Conceptos;
            i.complemento = comprobante.Complemento;

            return i;
        }
    }
}
