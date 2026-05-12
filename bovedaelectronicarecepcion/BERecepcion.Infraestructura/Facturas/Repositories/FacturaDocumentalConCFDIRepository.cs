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
using BERecepcion.Core.SAPPI.Dto;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using BERecepcion.Core.SAT.Dto;
using BERecepcion.Core.SAT.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class FacturaDocumentalConCFDIRepository : BaseSQLServerSqlRepository, IFacturaDocumentalConCFDIRepository
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
        private bool processBitacora = false;

        public FacturaDocumentalConCFDIRepository(string cnnString,
            IConfiguration configuration,
            IBitacoraRepository bitacoraRepository,
            ISAPPIRepository sapPIRepository,
            IFacturaElectronicaRepository facturaElectronicaRepository,
            IAdefasRepository adefasRepository,
            ISATRepository sATRepository,
            IInvoiceRepository invoiceRepository,
            ICFDIRepository cFDIRepository,
            IAnaliticoPagoRepository analiticoPagoRepository,
            ICorreoRepository correoRepository) : base(cnnString)
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
        }

        public async Task<InvoiceResultDto> SaveInvoice(string Organismo, string Reception, string Exercise, string RFCReceptor, string DocumentoBEId, ComprobanteBE comprobante, UsersDto User, string CFDIXML, string ViaPago, string Correo, IEnumerable<ComprobanteBE> notasCredito, IEnumerable<NotaCreditoCFDIXMLDto> notasCreditoXML, string comprobanteOriginal)
        {
            var result = await ValidateInvoice(Organismo, Reception, Exercise, RFCReceptor, DocumentoBEId, comprobante, User, CFDIXML, ViaPago, notasCredito, notasCreditoXML, Correo, comprobanteOriginal, comprobante.Version);

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
                var validacionError = await _correoRepository.NotificacionFacturaAPEmailAsync(Reception, validationErrors, User, Guid.Parse(DocumentoBEId), Correo, processBitacora, "FacturaDocumentalConCFDIRepository", "SaveInvoice");
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

        public async Task<InvoiceResultDto> ValidateInvoice(string Organismo, string Reception, string Exercise, string RFCReceptor, string DocumentoBEId, ComprobanteBE comprobante, UsersDto User, string CFDIXML, string ViaPago, IEnumerable<ComprobanteBE> notasCredito, IEnumerable<NotaCreditoCFDIXMLDto> notasCreditoCFDIXML, string Correo, string comprobanteOriginal, string CFDIVersion)
        {
            bool allowDuplicated = false;
            bool validateSAT = true;
            bool validatePEP = true;
            bool validateFull = true;
            bool validateVersion = false;

            CopadeDto copade = null;
            bool existsCriticalError = false;
            string sapError = "";
            Guid invoiceIdResult = Guid.Empty;
            string exceptionMessage = "";
            bool existedException = false;
            int status = -1;
            string receptorRFC = "";
            string sourceDocument = "";

            InvoiceDto dto = setInvoiceDto(comprobante, DocumentoBEId, "I", false, true, User.UserID, ViaPago);

            dto.UserId = User.UserID;
            dto.TipoComprobante = "I";
            dto.ViaPago = ViaPago;
            dto.CFDIVersion = CFDIVersion;

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
                validatePEP = Convert.ToBoolean(_configuration["InvoiceValidation:ValidarPEP"]);
                validateSAT = Convert.ToBoolean(_configuration["InvoiceValidation:ValidarSAT"]);
                validateFull = Convert.ToBoolean(_configuration["InvoiceValidation:ValidarFull"]);
                validateVersion = Convert.ToBoolean(_configuration["InvoiceValidation:ValidarVersion"]);
            }
            catch (Exception exx)
            {
                processBitacora = true;
                allowDuplicated = false;
                validateSAT = true;
                validateFull = true;
                validateVersion = false;
                Log.Error(exx.Message);
            }

            List<ValidationError> validationErrors = new List<ValidationError>();

            try
            {
                string xmlEncoded = CFDIXML;

                List<string> ncxml = new List<string>() { };
                if (notasCredito != null)
                {
                    foreach (var nc in notasCredito)
                    {
                        if (notasCreditoCFDIXML != null)
                        {
                            foreach (var ncxmlItem in notasCreditoCFDIXML)
                            {
                                if (ncxmlItem.UUID == nc.Complemento.TimbreFiscalDigital.UUID)
                                {
                                    string xmlNC = ncxmlItem.OriginalXML;
                                    ncxml.Add(xmlNC);
                                }
                            }
                        }
                    }
                }
                await _bitacoraRepository.logInitialRequest(Organismo, Reception, User.UserID.ToString(), xmlEncoded, ncxml, false, false);

                IEnumerable<ValidationError> errorCatalog = new List<ValidationError>() { };

                await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "60", User, processBitacora);
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

                await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "70", User, processBitacora);

                if (!existsCriticalError)
                {
                    if (!allowDuplicated)
                    {
                        // verificamos que no exista una factura
                        await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "160", User, processBitacora);
                        var _invoiceExists = _invoiceRepository.GetInvoiceByReception(Organismo, Reception, Exercise).Result;
                        if (_invoiceExists.Status != HttpStatusCode.OK)
                        {
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20422", "20422", Reception);
                            validationErrors[validationErrors.Count - 1].mensaje = _invoiceExists.Message;
                            existsCriticalError = true;
                        }
                        await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "161", User, processBitacora);
                        if (notasCredito != null)
                        {
                            foreach (var nc in notasCredito)
                            {
                                string sourceDocumentNC = setSourceDocumentNC(nc, Reception);
                                await _bitacoraRepository.BitacoraInvoice(Reception, "400", User, processBitacora);
                                var respNCUUID = await _facturaElectronicaRepository.GetInvoiceNotaCreditoByUUID(nc.Complemento.TimbreFiscalDigital.UUID, nc.Folio);
                                if (respNCUUID.Data != null)
                                {
                                    validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20423", "20423", sourceDocumentNC);
                                    existsCriticalError = true;
                                }
                                await _bitacoraRepository.BitacoraInvoice(Reception, "410", User, processBitacora);
                            }
                        }
                    }
                }

                //carga del copade
                await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "162", User, processBitacora);
                PICopadeRequestDto pi = new PICopadeRequestDto() { Clave = Organismo, Reception = Reception, Exercise = Exercise };
                var copadeResult = await _sapPIRepository.RecuperaCopadeBDAsync(pi);
                if (copadeResult.Status != HttpStatusCode.OK)
                {
                    validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12000", "12000", Reception);
                    validationErrors[validationErrors.Count - 1].mensaje = copadeResult.Message;
                    existsCriticalError = true;
                }
                else
                {
                    copade = copadeResult.Data;
                }
                await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "163", User, processBitacora);

                if (!existsCriticalError)
                {
                    Guid organismId = copade.OrganismID;
                    receptorRFC = await _cFDIRepository.GetOrganismRFC(organismId.ToString());
                }

                //validacion del invoice
                int qtyErrors = validationErrors.Count;
                validationErrors = await _cFDIRepository.validateDetailComprobante(Organismo, receptorRFC, errorCatalog, validationErrors, comprobante, copade, Reception, false, comprobanteOriginal);
                if (qtyErrors != validationErrors.Count)
                {
                    var er = validationErrors.Where(x => x.esTerminal).Count();
                    if (er > 0) existsCriticalError = true;
                }

                //validacion notas de credito
                if (notasCredito != null)
                {
                    int qtyNotasCredito = 0;
                    List<int> processed = new List<int>() { };
                    int pos = 0;
                    foreach (var nc in notasCredito)
                    {
                        qtyNotasCredito++;
                        string sourceDocumentNC = setSourceDocumentNC(nc, Reception);
                        await _bitacoraRepository.BitacoraInvoice(Reception, "120", User, processBitacora);
                        if (nc.CfdiRelacionados != null)
                        {
                            if (nc.CfdiRelacionados.CfdiRelacionado != null)
                            {
                                if (!string.IsNullOrEmpty(nc.CfdiRelacionados.CfdiRelacionado.UUID))
                                {
                                    if (comprobante.Complemento.TimbreFiscalDigital.UUID != Guid.Empty)
                                    {
                                        if (nc.CfdiRelacionados.CfdiRelacionado.UUID.ToUpper() != comprobante.Complemento.TimbreFiscalDigital.UUID.ToString().ToUpper())
                                        {
                                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20700", "20700", sourceDocumentNC);
                                            existsCriticalError = true;
                                        }
                                    }
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(copade.NotasCredito))
                        {
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12003", "12003", Reception);
                            existsCriticalError = true;
                        }
                        else
                        {
                            CopadeDto copadeNC = null;
                            processed = _cFDIRepository.setCopadeNC(copade.NotasCredito, nc, out copadeNC, processed);
                            if (copadeNC == null)
                            {
                                validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12004", "12004", sourceDocumentNC);
                                existsCriticalError = true;
                            }
                            else
                            {
                                validationErrors = await _cFDIRepository.validateDetailNCComprobante(Organismo, errorCatalog, validationErrors, nc, copadeNC, true, comprobante.Emisor.Rfc, receptorRFC, comprobante.Moneda, sourceDocumentNC);
                            }
                        }
                        pos++;
                        await _bitacoraRepository.BitacoraInvoice(Reception, "130", User, processBitacora);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(copade.NotasCredito))
                        {
                            CopadeNotasCredito cnc = JsonConvert.DeserializeObject<CopadeNotasCredito>(copade.NotasCredito);
                            if (cnc == null)
                            {
                                validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12006", "12006", Reception);
                                existsCriticalError = true;
                            }
                            else
                            {
                                int qtyCopadeNotaCredito = 0;
                                foreach (var cnct in cnc.notaCredito)
                                    qtyCopadeNotaCredito++;
                                if (qtyCopadeNotaCredito != qtyNotasCredito)
                                {
                                    validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12006", "12006", Reception);
                                    existsCriticalError = true;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12006", "12006", Reception);
                        existsCriticalError = true;
                        Log.Error(ex.Message);
                    }
                }

                dto.DiferencialCargo = Convert.ToDouble(dto.Total) < Convert.ToDouble(copade.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(copade.Total))), 2).ToString() : "0";
                dto.DiferencialAbono = Convert.ToDouble(dto.Total) > Convert.ToDouble(copade.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(copade.Total))), 2).ToString() : "0";
                dto.ImporteOriginal = copade.Total;

                if (validateVersion)
                {
                    foreach (var nc in notasCredito)
                    {
                        if (comprobante.Version != nc.Version)
                        {
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20800", "20800", sourceDocument);
                            existsCriticalError = true;
                        }
                    }

                    List<string> validVersions = await _cFDIRepository.GetValidVersions();
                    if (validVersions.Count > 0)
                    {
                        var x = validVersions.Where(x => x == comprobante.Version).ToList();
                        if (x == null || x.Count == 0)
                        {
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20900", "20900", sourceDocument);
                            existsCriticalError = true;
                        }
                        foreach (var nc in notasCredito)
                        {
                            var y = validVersions.Where(x => x == nc.Version);
                            if (y == null)
                            {
                                validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20800", "20800", sourceDocument);
                                existsCriticalError = true;
                            }
                        }
                    }
                }

                //validacion del SAT
                if (validateSAT)
                {
                    await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "420", User, processBitacora);
                    SATResultDto sATResultDto = await _cFDIRepository.setSATResult(User.CreditorRFC, RFCReceptor, dto.Total, dto.Uuid, validationErrors, errorCatalog, existsCriticalError, Reception);
                    existsCriticalError = sATResultDto.existsCriticalError;
                    validationErrors = sATResultDto.validationErrors;
                    await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "430", User, processBitacora);
                }

                await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "150", User, processBitacora);

                //validacion de Adefa
                //debe existir una preconfiguracion en Adefas para procesarlo
                string yearDocument = dto.InvoiceDate.Year.ToString();
                string yearCurrent = DateTime.Now.Year.ToString();
                var resultRecuperado = await _adefasRepository.GetValidaPeriodoAdefaAsync(Organismo, copade.Exercise, dto.InvoiceDate.ToString("yyyy-MM-dd"));
                if (resultRecuperado.Status == HttpStatusCode.OK)
                {
                    bool r = false;
                    if (resultRecuperado.Data != null)
                    {
                        r = resultRecuperado.Data.valido;
                    }
                    if (r == false)
                    {
                        validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20620", "20620", Reception);
                        existsCriticalError = true;
                    }
                }

                if (!existsCriticalError || validateFull && copade != null)
                {
                    //PEP Valida vigencia de las Fuentes de Financiamiento
                    if (validatePEP)
                    {
                        if (Organismo == "PEP")
                        {
                            validationErrors = await _cFDIRepository.ValidatePep(Organismo, errorCatalog, validationErrors, comprobante, copade, Reception, false);
                            var existError = validationErrors.Where(x => x.clave == "10500" || x.clave == "20500").FirstOrDefault();
                            if (existError != null) existsCriticalError = true;
                        }
                    }
                }

                //validacion CxP
                //llamada a la api
                await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "180", User, processBitacora);
                CXPDto cXP = setApiCXPDto(Organismo, copade, dto, User);
                var respApiCXP = await _sapPIRepository.PostCxP(cXP);
                await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "190", User, processBitacora);
                if (!existsCriticalError)
                {
                    string uuid = dto.Uuid.ToString();
                    CXPResultDto cXPResultDto = _cFDIRepository.setCxPResult(respApiCXP.Status, respApiCXP.Data, uuid, errorCatalog, validationErrors, Reception, existsCriticalError);
                    dto.SapDocument = cXPResultDto.sapDocument;
                    sapError = cXPResultDto.sapError;
                    validationErrors = cXPResultDto.validationErrors;
                    existsCriticalError = cXPResultDto.existsCriticalError;
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
                    if (notasCredito != null)
                    {
                        List<int> processed = new List<int>() { };
                        foreach (var nc in notasCredito)
                        {
                            CopadeDto copadeNC = null;
                            processed = _cFDIRepository.setCopadeNC(copade.NotasCredito, nc, out copadeNC, processed);
                            if (copadeNC != null)
                            {
                                InvoiceNotaCreditoDto iNC = _cFDIRepository.setInvoiceNotaCreditoDto(nc, Guid.Parse(DocumentoBEId), Guid.Empty, true, Convert.ToDouble(copadeNC.Total), Convert.ToDouble(copadeNC.Iva), nc.Total);
                                lrNC.Add(iNC);
                                if (notasCreditoCFDIXML != null)
                                {
                                    foreach (var ncc in notasCreditoCFDIXML)
                                    {
                                        if (iNC.Uuid == ncc.UUID)
                                        {
                                            lrNCXML.Add(ncc.OriginalXML);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "160", User, processBitacora);
                    InvoiceSaveGralDto invoiceSaveGralDto = new InvoiceSaveGralDto();
                    invoiceSaveGralDto.invoiceDto = dto;
                    invoiceSaveGralDto.invoiceXML = xmlEncoded;
                    invoiceSaveGralDto.invoiceCxPDto = iCxp;
                    invoiceSaveGralDto.invoiceNotasCreditoDto = lrNC;
                    invoiceSaveGralDto.InvoiceNotasCreditoXML = lrNCXML;
                    var respAdd = await _facturaElectronicaRepository.SaveInvoiceGral(invoiceSaveGralDto);
                    await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "170", User, processBitacora);
                    if (respAdd.Status != HttpStatusCode.OK || respAdd.Data == null)
                    {
                        validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "60003", "60003", Reception);
                    }
                    else
                    {
                        invoiceIdResult = respAdd.Data.invoiceDto.InvoiceId;
                        status = 1;
                    }
                }

                if (sapError != "")
                {
                    await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, sapError, User, processBitacora);
                }
                await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "330", User, processBitacora);

                if (!string.IsNullOrEmpty(Reception))
                {
                    if (validationErrors.Count > 0)
                    {
                        BitacoraInvoiceErrorInsertDto bie = new BitacoraInvoiceErrorInsertDto();
                        bie.reception = Reception;
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
                        await _cFDIRepository.setInitialRequestRejected(Reception, Correo, copade.CreditorRfc, RFCReceptor);
                    }
                }

                invoiceResultDto.Status = status;

            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
                existedException = true;
                Log.Error(ex.Message);

                try
                {
                    if (validationErrors.Count > 0)
                    {
                        BitacoraInvoiceErrorInsertDto biee = new BitacoraInvoiceErrorInsertDto();
                        biee.reception = Reception;
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

        private CXPDto setApiCXPDto(string Organismo, CopadeDto copade, InvoiceDto dto, UsersDto User)
        {
            CXPDto r = new CXPDto()
            {
                Organismo = Organismo,
                Entrada = copade.Reception,
                OrdenSap = copade.SapOrder,
                Ejercicio = copade.Exercise,
                FechaRecep = DateTime.Now.ToString("yyyy-MM-dd"),
                FechaFactura = dto.InvoiceDate.ToString("yyyy-MM-dd"),
                Factura = dto.Uuid == Guid.Empty ? string.Concat(dto.Serie, " ", dto.Folio) : dto.Uuid.ToString(),
                ViaPago = dto.Assignment,
                Usuario = User.Token,
                //ContratoVigente = copade.Contract,
                //Cliente = "",
                //Id_Analitico = "",
                //CentroGestor = copade.Center,
                ImporteFactura = dto.Total,
                ImporteOriginal = copade.Total,
                DiferencialCargo = Convert.ToDouble(dto.Total) < Convert.ToDouble(copade.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(copade.Total))), 2).ToString() : "0",     // pendiente la regla de diferencial cargo/abono
                DiferencialAbono = Convert.ToDouble(dto.Total) > Convert.ToDouble(copade.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(copade.Total))), 2).ToString() : "0"
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

        public InvoiceDto setInvoiceDto(ComprobanteBE comprobante, string DocumentoBEId, string tipoComprobante, bool isElectronicReception, bool isCopade, Guid UserId, string viaPago)
        {
            InvoiceDto dto = new InvoiceDto();

            double diferencialAbono, diferencialCargo;
            diferencialAbono = diferencialCargo = 0;
            dto.UserId = UserId;
            dto.Assignment = viaPago;
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
            dto.ViaPago = viaPago;
            dto.CartaPorte = comprobante.Complemento.CartaPorte;

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
            if (comprobante.Addenda != null)
            {
                if (comprobante.Addenda.Addenda_Pemex != null)
                {
                    i.ejercicio = comprobante.Addenda.Addenda_Pemex.EJERCICIO;
                }
            }

            return i;
        }

        private string setSourceDocument(ComprobanteBE comprobante, string originalReception)
        {
            string result, reception, UUID, serie, folio;
            result = reception = UUID = serie = folio = "";

            if (comprobante.Addenda != null)
            {
                if (comprobante.Addenda.Addenda_Pemex != null)
                {
                    reception = comprobante.Addenda.Addenda_Pemex.ENTRADA.ToString();
                }
                else
                {
                    reception = originalReception;
                }
            }
            else
            {
                reception = originalReception;
            }
            if (comprobante.Complemento != null)
            {
                if (comprobante.Complemento.TimbreFiscalDigital != null)
                {
                    if (comprobante.Complemento.TimbreFiscalDigital.UUID != Guid.Empty)
                    {
                        UUID = comprobante.Complemento.TimbreFiscalDigital.UUID.ToString();
                    }
                }
            }
            if (!string.IsNullOrEmpty(comprobante.Serie))
            {
                serie = comprobante.Serie;
            }
            if (!string.IsNullOrEmpty(comprobante.Folio))
            {
                folio = comprobante.Folio;
            }

            result = reception + " ";
            if (UUID == "")
            {
                result = result + " " + serie + " " + folio;
            }
            else
            {
                result = result + " " + UUID;
            }

            return result.Trim();
        }

        private string setSourceDocumentNC(ComprobanteBE comprobante, string originalReception)
        {
            return setSourceDocument(comprobante, originalReception);
        }
    }
}
