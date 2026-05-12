using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
    public class FacturaDocumentalSinCFDIRepository : BaseSQLServerSqlRepository, IFacturaDocumentalSinCFDIRepository
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

        public FacturaDocumentalSinCFDIRepository(string cnnString,
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

        public async Task<InvoiceResultDto> SaveInvoice(string Organismo, string Reception, string Exercise, string RFCReceptor, string Correo, InvoiceDto dto, UsersDto User)
        {
            var f = dto.InvoiceDate;
            dto.InvoiceDate = new DateTime(f.Year, f.Month, f.Day, 0, 0, 0);
            if (dto.InvoiceNotaCredito != null)
            {
                foreach (var nc in dto.InvoiceNotaCredito)
                {
                    var fnc = nc.NotaCreditoDate;
                    nc.NotaCreditoDate = new DateTime(fnc.Year, fnc.Month, fnc.Day, 0, 0, 0);
                }
            }
            var result = await ValidateInvoice(Organismo, Reception, Exercise, RFCReceptor, Correo, dto, User);

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
                var validacionError = await _correoRepository.NotificacionFacturaAPEmailAsync(Reception, validationErrors, User, dto.DocumentoBEId, Correo, processBitacora, "FacturaDocumentalSinCFDIRepository", "SaveInvoice");
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

        public async Task<InvoiceResultDto> ValidateInvoice(string Organismo, string Reception, string Exercise, string RFCReceptor, string Correo, InvoiceDto dto, UsersDto User)
        {
            bool allowDuplicated = false;
            bool validateSAT = true;
            bool validatePEP = true;
            CopadeDto copade = null;
            bool existsCriticalError = false;
            string sapError = "";
            Guid invoiceIdResult = Guid.Empty;
            string exceptionMessage = "";
            bool existedException = false;
            int status = -1;

            dto.UserId = User.UserID;
            dto.TipoComprobante = "I";
            dto.Subtotal = "0";
            dto.Total = dto.Total.Replace(",", "");

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
                validatePEP = Convert.ToBoolean(_configuration["InvoiceValidation:ValidarPEP"]);
            }
            catch (Exception exx)
            {
                processBitacora = true;
                allowDuplicated = false;
                validateSAT = true;
                Log.Error(exx.Message);
            }

            List<ValidationError> validationErrors = new List<ValidationError>();

            try
            {
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

                await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "140", User, processBitacora);

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

                //registro log inicial y creacion xml

                string xmlEncoded = setXml(dto, copade);

                await _bitacoraRepository.logInitialRequest(Organismo, Reception, User.UserID.ToString(), xmlEncoded, new List<string>() { }, false, false);

                //validacion del total
                if (Math.Abs(Convert.ToDouble(copade.Total) - Convert.ToDouble(dto.Total)) >= Convert.ToDouble(1))
                {
                    validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20402", "20402", Reception);
                    existsCriticalError = true;
                }
                if (dto.InvoiceDate > DateTime.Now)
                {
                    validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "10600", "10600", Reception);
                    existsCriticalError = true;
                }
                //validacion de las notas de credito
                if (dto.InvoiceNotaCredito != null)
                {
                    foreach (var nc in dto.InvoiceNotaCredito)
                    {
                        validationErrors = validateInvoiceNotaCredito(copade, nc, errorCatalog, validationErrors, Reception);
                    }
                }

                dto.ImporteOriginal = copade.Total;
                dto.Subtotal = copade.Subtotal;
                dto.DiferencialCargo = Convert.ToDouble(dto.Total) < Convert.ToDouble(copade.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(copade.Total))), 2).ToString() : "0";
                dto.DiferencialAbono = Convert.ToDouble(dto.Total) > Convert.ToDouble(copade.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(copade.Total))), 2).ToString() : "0";
                dto.TotalImpuestosTrasladados = copade.TaxTotalTransferred;
                dto.TotalImpuestosRetenidos = copade.TaxTotalRetained;

                if (dto.Uuid != Guid.Empty)
                {
                    var rslt = await _facturaElectronicaRepository.GetInvoiceByUUID(dto.Uuid);
                    if (rslt != null)
                    {
                        if (rslt.Data != null)
                        {
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12209", "12209", Reception);
                            existsCriticalError = true;
                        }
                    }
                }

                if (string.IsNullOrEmpty(Correo))
                {
                    validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12210", "12210", Reception);
                    existsCriticalError = true;
                }

                //validacion del SAT
                if (validateSAT && dto.Uuid != Guid.Empty)
                {
                    await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "420", User, processBitacora);
                    SATResultDto sATResultDto = await _cFDIRepository.setSATResult(User.CreditorRFC, RFCReceptor, dto.Total, dto.Uuid, validationErrors, errorCatalog, existsCriticalError, Reception);
                    existsCriticalError = sATResultDto.existsCriticalError;
                    validationErrors = sATResultDto.validationErrors;
                    await _bitacoraRepository.BitacoraInvoiceAP(Reception, dto.DocumentoBEId, "430", User, processBitacora);
                }

                //PEP Valida vigencia de las Fuentes de Financiamiento
                if (validatePEP)
                {
                    if (Organismo == "PEP")
                    {
                        string fechaPep = dto.InvoiceDate.Year + "-" + dto.InvoiceDate.Month.ToString().PadLeft(2, '0') + "-" + dto.InvoiceDate.Day.ToString().PadLeft(2, '0');
                        validationErrors = await ValidatePep(Organismo, errorCatalog, validationErrors, fechaPep, copade, Reception, false);
                        var existError = validationErrors.Where(x => x.clave == "10500" || x.clave == "20500").FirstOrDefault();
                        if (existError != null) existsCriticalError = true;
                    }
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
                    if (dto.InvoiceNotaCredito != null)
                    {
                        foreach (var nc in dto.InvoiceNotaCredito)
                        {
                            lrNC.Add(nc);
                            lrNCXML.Add(serializeNotaCreditoXML(nc, copade));
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
                ImporteFactura = dto.Total,
                ImporteOriginal = copade.Total,
                DiferencialCargo = Convert.ToDouble(dto.Total) < Convert.ToDouble(copade.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(copade.Total))), 2).ToString() : "0",     // pendiente la regla de diferencial cargo/abono
                DiferencialAbono = Convert.ToDouble(dto.Total) > Convert.ToDouble(copade.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(copade.Total))), 2).ToString() : "0"
            };

            return r;
        }

        private ComprobanteBE40 setInvoiceForXmlDto(InvoiceDto dto, CopadeDto copade)
        {
            ComprobanteBE40 xDto = new ComprobanteBE40();

            xDto.Emisor = new ComprobanteEmisor()
            {
                Rfc = copade.CreditorRfc,
                Nombre = copade.Creditor
            };

            xDto.Receptor = new ComprobanteReceptor()
            {
                Rfc = copade.Rfc,
                Nombre = copade.Society
            };

            xDto.Addenda = new ComprobanteAddenda()
            {
                Addenda_Pemex = new AddendaAddenda_Pemex()
                {
                    CONTRATO = copade.Contract,
                    VUREGION = dto.Assignment,
                    EJERCICIO = copade.Exercise,
                    ENTRADA = copade.Reception,
                    O_SURTIMIENTO = copade.SapOrder,
                    N_ESTIMACION = copade.EstimatedNumber,
                    N_ACREEDOR = copade.CreditorNumber,
                    MONEDA = copade.Currency
                }
            };

            List<ComprobanteConcepto> conc = new List<ComprobanteConcepto>() { };
            if (copade.vPreFactura.comprobante.conceptos != null)
            {
                foreach (var item in copade.vPreFactura.comprobante.conceptos)
                {
                    ComprobanteConcepto i = new ComprobanteConcepto()
                    {
                        Importe = Convert.ToDecimal(item.importe),
                        Cantidad = Convert.ToDecimal(item.cantidad),
                        Descripcion = item.descripcion,
                        ValorUnitario = Convert.ToDecimal(item.valorUnitario)
                    };
                    conc.Add(i);
                }
            }

            xDto.Conceptos = conc.ToArray();

            xDto.Moneda = copade.Currency;
            xDto.Fecha = Convert.ToDateTime(dto.InvoiceDate);
            xDto.Folio = dto.Folio;
            xDto.Serie = dto.Serie;
            xDto.SubTotal = Convert.ToDecimal(copade.Subtotal.Replace(",", ""));
            xDto.TipoDeComprobante = "I";
            xDto.Total = Convert.ToDecimal(copade.Total.Replace(",", ""));
            xDto.Impuestos = new ComprobanteImpuestos();
            xDto.Impuestos.TotalImpuestosTrasladados = Convert.ToDecimal(copade.Iva.Replace(",", ""));
            xDto.Impuestos.TotalImpuestosRetenidos = 0;
            xDto.Impuestos.TotalImpuestosTrasladadosSpecified = true;
            xDto.Impuestos.TotalImpuestosRetenidosSpecified = true;
            
            xDto.Version = "4.0";
            
            return xDto;
        }

        private ComprobanteBE40 setNCForXmlDto(InvoiceNotaCreditoDto dto, CopadeDto copade)
        {
            ComprobanteBE40 xDto = new ComprobanteBE40();
            xDto.Emisor = new ComprobanteEmisor()
            {
                Rfc = copade.CreditorRfc,
                Nombre = copade.Creditor
            };

            xDto.Receptor = new ComprobanteReceptor()
            {
                Rfc = copade.Rfc,
                Nombre = copade.Society
            };

            xDto.Addenda = new ComprobanteAddenda()
            {
                Addenda_Pemex = new AddendaAddenda_Pemex()
                {
                    CONTRATO = copade.Contract,
                    VUREGION = copade.Assignment,
                    EJERCICIO = copade.Exercise,
                    ENTRADA = copade.Reception,
                    O_SURTIMIENTO = copade.SapOrder,
                    N_ESTIMACION = copade.EstimatedNumber,
                    N_ACREEDOR = copade.CreditorNumber,
                    MONEDA = copade.Currency
                }
            };

            if (dto.Uuid != null)
            {
                xDto.Complemento = new ComprobanteComplemento()
                {
                    TimbreFiscalDigital = new TimbreFiscalDigital()
                    {
                        UUID = Guid.Parse(dto.Uuid.ToString())
                    }
                };
            }

            List<ComprobanteConcepto> conc = new List<ComprobanteConcepto>() { };
            ComprobanteConcepto item = new ComprobanteConcepto()
            {
                Cantidad = Convert.ToDecimal(dto.Amount.Replace(",", "")),
                Descripcion = dto.Descripcion.Replace("(NC)", "").Trim(),
                Importe = Convert.ToDecimal(dto.Total.Replace(",", "")),
                ValorUnitario = Convert.ToDecimal(dto.Total.Replace(",", ""))
            };
            conc.Add(item);

            xDto.Conceptos = conc.ToArray();
            xDto.Fecha = Convert.ToDateTime(dto.NotaCreditoDate);
            xDto.Folio = dto.Folio;
            xDto.Serie = dto.Serie;
            xDto.TipoDeComprobante = "N";
            xDto.Impuestos = new ComprobanteImpuestos();
            xDto.Impuestos.TotalImpuestosTrasladados = Convert.ToDecimal(dto.Iva.Replace(",", ""));
            xDto.Impuestos.TotalImpuestosRetenidos = 0;
            xDto.Impuestos.TotalImpuestosTrasladadosSpecified = true;
            xDto.Impuestos.TotalImpuestosRetenidosSpecified = true;
            xDto.SubTotal = Math.Round(Convert.ToDecimal(dto.Total) - Convert.ToDecimal(dto.Iva), 8);
            xDto.Total = Convert.ToDecimal(dto.Total.Replace(",",""));
            xDto.Moneda = copade.Currency;
            xDto.Version = "4.0";
            
            return xDto;
        }

        private string setXml(InvoiceDto dto, CopadeDto copade)
        {
            string xmlEncoded = "";
            ComprobanteBE40 c = setInvoiceForXmlDto(dto, copade);

            XmlSerializer ser = new XmlSerializer(typeof(ComprobanteBE40));
            using (MemoryStream ms = new MemoryStream())
            using (XmlTextWriter tw = new XmlTextWriter(ms, Encoding.UTF8))
            {
                tw.Formatting = System.Xml.Formatting.Indented;
                ser.Serialize(tw, c);
                xmlEncoded = Encoding.UTF8.GetString(ms.ToArray());
            }

            return xmlEncoded;
        }

        public async Task<List<ValidationError>> ValidatePep(string claveOrganismo, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, string Fecha, CopadeDto copade, string documento, bool esNotaCredito)
        {
            try
            {
                string fechaRecepcion = Fecha.Substring(0, 10);
                ValidacionCXPDto dtoMiro = new ValidacionCXPDto() { Contrato = copade.Contract, Fecha = fechaRecepcion, Organismo = claveOrganismo };
                var r = await _sapPIRepository.PostValidacionCxP(dtoMiro);
                if (r.Data.Status == false)
                {
                    _cFDIRepository.addError(esNotaCredito, catalogErrors, validationErrors, "10500", "20500", documento);
                    if (!string.IsNullOrEmpty(r.Data.Mensaje))
                    {
                        validationErrors[validationErrors.Count - 1].descripcion = r.Data.Mensaje;
                        validationErrors[validationErrors.Count - 1].mensaje = r.Data.Mensaje;
                    }
                }
            }
            catch (Exception ex)
            {
                _cFDIRepository.addError(esNotaCredito, catalogErrors, validationErrors, "10500", "20500", documento);
                Log.Error(ex.Message);
            }

            return validationErrors;
        }

        private string serializeNotaCreditoXML(InvoiceNotaCreditoDto dto, CopadeDto copade)
        {
            string xmlEncoded = "";

            ComprobanteBE40 c = setNCForXmlDto(dto, copade);
            XmlSerializer ser = new XmlSerializer(typeof(ComprobanteBE40));
            using (MemoryStream ms = new MemoryStream())
            using (XmlTextWriter tw = new XmlTextWriter(ms, Encoding.UTF8))
            {
                tw.Formatting = System.Xml.Formatting.Indented;
                ser.Serialize(tw, c);
                xmlEncoded = Encoding.UTF8.GetString(ms.ToArray());
            }

            return xmlEncoded;
        }

        private List<ValidationError> validateInvoiceNotaCredito(CopadeDto copade, InvoiceNotaCreditoDto nc, IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, string Reception)
        {
            if (string.IsNullOrEmpty(nc.Folio) && nc.Uuid == Guid.Empty)
            {
                validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12007", "12007", Reception);
            }

            return validationErrors;
        }
    }
}
