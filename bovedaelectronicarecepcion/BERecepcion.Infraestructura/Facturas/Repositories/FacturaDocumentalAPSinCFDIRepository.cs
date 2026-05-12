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
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using BERecepcion.Core.SAT.Dto;
using BERecepcion.Core.SAT.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class FacturaDocumentalAPSinCFDIRepository : BaseSQLServerSqlRepository, IFacturaDocumentalAPSinCFDIRepository
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
        private readonly ICatalogosRepository _catalogosRepository;
        private bool processBitacora = false;

        public FacturaDocumentalAPSinCFDIRepository(string cnnString,
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
            ICatalogosRepository catalogosRepository) : base(cnnString)
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
            _catalogosRepository = catalogosRepository;
        }

        public async Task<InvoiceResultDto> SaveInvoice(string Organismo, string IdAnaliticoPago, string RFCReceptor, string Correo, InvoiceDto dto, UsersDto User)
        {
            var f = dto.InvoiceDate;
            dto.InvoiceDate = new DateTime(f.Year, f.Month, f.Day, 0, 0, 0);
            var result = await ValidateInvoice(Organismo, IdAnaliticoPago, RFCReceptor, Correo, dto, User);

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
                var validacionError = await _correoRepository.NotificacionFacturaAPEmailAsync(IdAnaliticoPago, validationErrors, User, dto.DocumentoBEId, Correo, processBitacora, "FacturaDocumentalAPSinCFDIRepository", "SaveInvoice");
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

        public async Task<InvoiceResultDto> ValidateInvoice(string Organismo, string IdAnaliticoPago, string RFCReceptor, string Correo, InvoiceDto dto, UsersDto User)
        {
            bool allowDuplicated = false;
            bool validateSAT = true;
            AnaliticoPagoDto analitico = null;
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

                //datos complementarios
                dto.Subtotal = analitico.SubTotal;
                dto.ImporteOriginal = analitico.Total;
                dto.TotalImpuestosRetenidos = analitico.vImpuestos.totalImpuestosRetenidos;
                dto.TotalImpuestosTrasladados = analitico.vImpuestos.totalImpuestosTrasladados;

                var organismos = _catalogosRepository.GetOrganismosAsync().Result;
                var org = organismos.Data.ToList().Where(x => x.Clave == Organismo).FirstOrDefault();
                string xmlEncoded = setXml(dto, analitico, org);

                await _bitacoraRepository.logInitialRequest(Organismo, IdAnaliticoPago, User.UserID.ToString(), xmlEncoded, new List<string>() { }, false, false);

                //validacion del total
                if (Math.Abs(Convert.ToDouble(analitico.Total) - Convert.ToDouble(dto.Total)) >= Convert.ToDouble(1))
                {
                    validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "10403", "10403", IdAnaliticoPago);
                    existsCriticalError = true;
                }
                if (dto.InvoiceDate > DateTime.Now)
                {
                    validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "10600", "10600", IdAnaliticoPago);
                    existsCriticalError = true;
                }
                dto.DiferencialCargo = Convert.ToDouble(dto.Total) < Convert.ToDouble(analitico.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(analitico.Total))), 2).ToString() : "0";
                dto.DiferencialAbono = Convert.ToDouble(dto.Total) > Convert.ToDouble(analitico.Total) ? (Math.Round(Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(analitico.Total))), 2).ToString() : "0";

                if (dto.Uuid != Guid.Empty)
                {
                    var rslt = await _facturaElectronicaRepository.GetInvoiceByUUID(dto.Uuid);
                    if (rslt != null)
                    {
                        if (rslt.Data != null)
                        {
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12209", "12209", IdAnaliticoPago);
                            existsCriticalError = true;
                        }
                    }
                }

                if (string.IsNullOrEmpty(Correo))
                {
                    validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12210", "12210", IdAnaliticoPago);
                    existsCriticalError = true;
                }

                //validacion del SAT
                if (validateSAT && dto.Uuid != Guid.Empty)
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

        private ComprobanteBE40 setInvoiceForXmlDto(InvoiceDto dto, AnaliticoPagoDto analitico, OrganismDto organism)
        {
            ComprobanteBE40 xDto = new ComprobanteBE40();

            xDto.Emisor = new ComprobanteEmisor()
            {
                Rfc = analitico.CreditorRFC,
                Nombre = analitico.Creditor
            };

            xDto.Receptor = new ComprobanteReceptor()
            {
                Rfc = organism.Rfc,
                Nombre = organism.Name
            };

            xDto.Addenda = new ComprobanteAddenda()
            {
                Addenda_Pemex = new AddendaAddenda_Pemex()
                {
                    CONTRATO = analitico.Contrato,
                    VUREGION = dto.Assignment,
                    EJERCICIO = analitico.Ejercicio,
                    ENTRADA = analitico.IdAnalitico,
                    O_SURTIMIENTO = "",
                    N_ESTIMACION = "",
                    N_ACREEDOR = analitico.NumAcreedor,
                    MONEDA = analitico.Moneda
                }
            };

            List<ComprobanteConcepto> conc = new List<ComprobanteConcepto>() { };
            if (analitico.vPreFactura.comprobante.conceptos != null)
            {
                foreach (var item in analitico.vPreFactura.comprobante.conceptos)
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

            xDto.Moneda = analitico.Moneda;
            xDto.Fecha = Convert.ToDateTime(dto.InvoiceDate);
            xDto.Folio = dto.Folio;
            xDto.Serie = dto.Serie;
            xDto.SubTotal = Convert.ToDecimal(analitico.SubTotal.Replace(",", ""));
            xDto.TipoDeComprobante = "I";
            xDto.Total = Convert.ToDecimal(analitico.Total.Replace(",", ""));
            xDto.Impuestos = new ComprobanteImpuestos();
            xDto.Impuestos.TotalImpuestosTrasladados = Convert.ToDecimal(analitico.Iva.Replace(",", ""));
            xDto.Impuestos.TotalImpuestosRetenidos = 0;
            xDto.Impuestos.TotalImpuestosTrasladadosSpecified = true;
            xDto.Impuestos.TotalImpuestosRetenidosSpecified = true;

            xDto.Version = "4.0";

            return xDto;
        }

        private string setXml(InvoiceDto dto, AnaliticoPagoDto analitico, OrganismDto organism)
        {
            string xmlEncoded = "";
            ComprobanteBE40 xmlDto = setInvoiceForXmlDto(dto, analitico, organism);

            XmlSerializer ser = new XmlSerializer(typeof(ComprobanteBE40));
            using (MemoryStream ms = new MemoryStream())
            using (XmlTextWriter tw = new XmlTextWriter(ms, Encoding.UTF8))
            {
                tw.Formatting = System.Xml.Formatting.Indented;
                ser.Serialize(tw, xmlDto);
                xmlEncoded = Encoding.UTF8.GetString(ms.ToArray());
            }

            return xmlEncoded;
        }
    }
}
