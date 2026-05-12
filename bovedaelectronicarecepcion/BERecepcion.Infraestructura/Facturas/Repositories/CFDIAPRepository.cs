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
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using BERecepcion.Core.SAT.Dto;
using BERecepcion.Core.SAT.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Admin.Dto;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class CFDIAPRepository : BaseSQLServerSqlRepository, ICFDIAPRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly ISAPPIRepository _sapPIRepository;
        private readonly IFacturaElectronicaRepository _facturaElectronicaRepository;
        private readonly IAdefasRepository _adefasRepository;
        private readonly ISATRepository _sATRepository;
        private readonly ICorreoRepository _correoRepository;
        private readonly IUsuariosRepository _usuariosRepository;
        private readonly IAnaliticoPagoRepository _analiticoPagoRepository;
        private readonly ICFDIRepository _cFDIRepository;
        private bool processBitacora = false;

        public CFDIAPRepository(string cnnString, IConfiguration configuration, IBitacoraRepository bitacoraRepository,
            ISAPPIRepository sapPIRepository, IFacturaElectronicaRepository facturaElectronicaRepository,
            IAdefasRepository adefasRepository, ISATRepository sATRepository, ICorreoRepository correoRepository,
            IUsuariosRepository usuariosRepository, IAnaliticoPagoRepository analiticoPagoRepository,
            ICFDIRepository cFDIRepository) : base(cnnString)
        {
            _configuration = configuration;
            _bitacoraRepository = bitacoraRepository;
            _sapPIRepository = sapPIRepository;
            _facturaElectronicaRepository = facturaElectronicaRepository;
            _adefasRepository = adefasRepository;
            _sapPIRepository = sapPIRepository;
            _sATRepository = sATRepository;
            _correoRepository = correoRepository;
            _usuariosRepository = usuariosRepository;
            _analiticoPagoRepository = analiticoPagoRepository;
            _cFDIRepository = cFDIRepository;
        }

        public InvoiceDto setInvoiceDto(InvoiceResultDto comprobante, AnaliticoPagoDto analitico, string tipoComprobante, bool isElectronicReception, bool isCopade, Guid UserId, string ViaPago)
        {
            InvoiceDto dto = new InvoiceDto();

            double diferencialAbono, diferencialCargo;
            diferencialAbono = diferencialCargo = 0;
            if (string.IsNullOrEmpty(analitico.Total)) analitico.Total = "0";
            if (string.IsNullOrEmpty(analitico.SubTotal)) analitico.SubTotal = "0";

            if (Convert.ToDouble(comprobante.comprobante.Total) < Convert.ToDouble(analitico.Total)) diferencialCargo = Math.Round(Math.Abs(Convert.ToDouble(comprobante.comprobante.Total) - Convert.ToDouble(analitico.Total)), 2);
            if (Convert.ToDouble(comprobante.comprobante.Total) > Convert.ToDouble(analitico.Total)) diferencialAbono = Math.Round(Math.Abs(Convert.ToDouble(comprobante.comprobante.Total) - Convert.ToDouble(analitico.Total)), 2);

            dto.UserId = UserId;
            dto.Assignment = ViaPago;
            dto.OriginalXML = comprobante.comprobante.Cfdi;
            dto.CompensationDocument = "";
            dto.DiferencialAbono = diferencialAbono.ToString();
            dto.DiferencialCargo = diferencialCargo.ToString();
            dto.DocumentoBEId = analitico.AnaliticoPagoID;
            dto.ElectronicReception = "E";
            dto.EmailProvider = "";
            dto.EmailProviderSendDate = DateTime.Now;
            dto.Estatus = "0";
            dto.FechaEmision = null;
            dto.Folio = comprobante.comprobante.Folio;
            dto.ImporteOriginal = analitico.Total;
            dto.InvoiceDate = convertToDate(comprobante.comprobante.Fecha);
            dto.InvoiceId = Guid.Empty;
            dto.IsCopade = isCopade;
            dto.LastStatus = "0";
            dto.LastStatusDate = DateTime.Now;
            dto.ReceptionDate = DateTime.Now;
            dto.RutaArchivo = "";
            dto.SapDocument = "";
            dto.Serie = comprobante.comprobante.Serie;
            dto.Subtotal = comprobante.comprobante.SubTotal.ToString();
            dto.TipoComprobante = tipoComprobante;
            dto.Total = comprobante.comprobante.Total.ToString();
            dto.TotalImpuestosRetenidos = comprobante.comprobante.Impuestos.TotalImpuestosRetenidos.ToString();
            dto.TotalImpuestosTrasladados = comprobante.comprobante.Impuestos.TotalImpuestosTrasladados.ToString();
            dto.Uuid = comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID;
            dto.ViaPago = ViaPago;
            dto.CartaPorte = comprobante.comprobante.Complemento.CartaPorte;
            dto.CFDIVersion = comprobante.comprobante.Version;

            return dto;
        }

        public CXPDto setApiCXPDto(InvoiceResultDto comprobante, AnaliticoPagoDto analitico, string tipoComprobante, bool isElectronicReception, bool isCopade, string token)
        {
            CXPDto dto = new CXPDto();

            double diferencialAbono, diferencialCargo;
            diferencialAbono = diferencialCargo = 0;
            if (string.IsNullOrEmpty(analitico.Total)) analitico.Total = "0";
            if (string.IsNullOrEmpty(analitico.SubTotal)) analitico.SubTotal = "0";

            if (Convert.ToDouble(comprobante.comprobante.Total) < Convert.ToDouble(analitico.Total)) diferencialCargo = Math.Round(Math.Abs(Convert.ToDouble(comprobante.comprobante.Total) - Convert.ToDouble(analitico.Total)), 2);
            if (Convert.ToDouble(comprobante.comprobante.Total) > Convert.ToDouble(analitico.Total)) diferencialAbono = Math.Round(Math.Abs(Convert.ToDouble(comprobante.comprobante.Total) - Convert.ToDouble(analitico.Total)), 2);

            dto.CentroGestor = "";
            dto.Cliente = "";
            dto.ContratoVigente = "";
            dto.Cliente = "";
            dto.DiferencialAbono = diferencialAbono.ToString();
            dto.DiferencialCargo = diferencialCargo.ToString();
            dto.DocumentoSAP = "";
            dto.Ejercicio = analitico.Ejercicio;
            dto.Entrada = analitico.IdAnalitico;
            dto.Factura = comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID.ToString();
            dto.FechaEmision = null;
            dto.FechaFactura = comprobante.comprobante.Fecha;
            dto.FechaRecep = DateTime.Now.ToString("yyyy-MM-dd");
            dto.Id_Analitico = "";
            dto.ImporteFactura = comprobante.comprobante.Total.ToString();
            dto.ImporteOriginal = analitico.Total;
            dto.Mensaje = "";
            dto.OrdenSap = "";
            dto.Organismo = "SIIC";
            dto.Res = "";
            dto.Usuario = token;
            dto.ViaPago = comprobante.comprobante.Addenda.Addenda_Pemex.VUREGION;

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

        public string setSourceDocument(ComprobanteBE comprobante, string IdAnaliticoPago)
        {
            string result, UUID, serie, folio;
            result = UUID = serie = folio = "";

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

            result = IdAnaliticoPago + " ";
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

        public async Task<InvoiceResultDto> processSaveInvoice(InvoiceResultDto comprobante)
        {
            if (comprobante.comprobante == null)
                return null;

            IEnumerable<ValidationError> errorCatalog = new List<ValidationError>() { };
            List<ValidationError> validationErrors = new List<ValidationError>();
            AnaliticoPagoDto analitico = null;
            bool existsCriticalError = false;
            bool allowDuplicated = false;
            bool validateSAT = true;
            bool validateFull = true;
            bool validateVersion = false;
            bool existedException = false;
            string exceptionMessage = "";
            UsersDto User = comprobante.User;
            Guid invoiceIdResult = Guid.Empty;
            string sourceDocument = "";

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
                validateFull = Convert.ToBoolean(_configuration["InvoiceValidation:ValidarFull"]);
                validateVersion = Convert.ToBoolean(_configuration["InvoiceValidation:ValidarVersion"]);
            }
            catch (Exception exx)
            {
                processBitacora = true;
                allowDuplicated = false;
                validateFull = true;
                validateVersion = false;
                Log.Error(exx.Message);
            }

            try
            {
                string claveOrganismo, receptorRFC;
                claveOrganismo = receptorRFC = "";

                claveOrganismo = comprobante.Organismo;

                if (!existsCriticalError)
                {
                    if (comprobante.comprobante == null)
                    {
                        comprobante.Status = -1;
                        validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "00000", "00000", sourceDocument);
                        existsCriticalError = true;
                    }
                }

                ComprobanteDto compr = new ComprobanteDto() { OriginalXml = comprobante.OriginalXML, claveOrganismo = comprobante.Organismo, esCopade = false, esDocumental = false, User = comprobante.User };
                await _cFDIRepository.logInitialRequest(compr);

                if (!existsCriticalError)
                {
                    sourceDocument = setSourceDocument(comprobante.comprobante, comprobante.IdDocumento);
                }

                if (!existsCriticalError)
                {
                    await _bitacoraRepository.BitacoraInvoiceAP(comprobante.IdDocumento, Guid.Parse(comprobante.DocumentoBEId), "60", User, processBitacora);
                    var rslt = await _cFDIRepository.GetValidationErrorCatalog();
                    if (rslt.Status != HttpStatusCode.OK)
                    {
                        comprobante.Status = -1;
                        validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "90000", "90000", sourceDocument);
                        existsCriticalError = true;
                    }
                    else
                    {
                        errorCatalog = rslt.Data;
                        if (errorCatalog == null)
                        {
                            comprobante.Status = -1;
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "90001", "90001", sourceDocument);
                            existsCriticalError = true;
                        }
                    }
                    await _bitacoraRepository.BitacoraInvoiceAP(comprobante.IdDocumento, Guid.Parse(comprobante.DocumentoBEId), "70", User, processBitacora);
                }

                if (!existsCriticalError)
                {
                    await _bitacoraRepository.BitacoraInvoiceAP(comprobante.IdDocumento, Guid.Parse(comprobante.DocumentoBEId), "40", User, processBitacora);
                    var analiticoResult = await _analiticoPagoRepository.GetAPByIdAnaliticoAsync(comprobante.IdDocumento);
                    if (analiticoResult.Status != HttpStatusCode.OK)
                    {
                        comprobante.Status = -1;
                        validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12200", "12200", sourceDocument);
                        existsCriticalError = true;
                    }
                    else
                    {
                        if (((DataResult<AnaliticoPagoDto>)analiticoResult).Data == null)
                        {
                            comprobante.Status = -1;
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12200", "12200", sourceDocument);
                            existsCriticalError = true;
                        }
                        else
                        {
                            analitico = analiticoResult.Data;
                            if (analitico == null)
                            {
                                comprobante.Status = -1;
                                validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12200", "12200", sourceDocument);
                                existsCriticalError = true;
                            }
                            else
                            {
                                if (analitico.IsCancel == true)
                                {
                                    comprobante.Status = -1;
                                    validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12201", "12201", sourceDocument);
                                    existsCriticalError = true;
                                }
                                else
                                {
                                    if (analitico.FunctionarySignDate == null)
                                    {
                                        comprobante.Status = -1;
                                        validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "12208", "12208", sourceDocument);
                                        existsCriticalError = true;
                                    }
                                }
                            }
                        }
                    }
                    await _bitacoraRepository.BitacoraInvoiceAP(comprobante.IdDocumento, Guid.Parse(comprobante.DocumentoBEId), "50", User, processBitacora);
                }

                if (!existsCriticalError || validateFull == true && analitico != null)
                {
                    Guid organismId = analitico.OrganismID;
                    receptorRFC = await _cFDIRepository.GetOrganismRFC(organismId.ToString());
                }

                if (!existsCriticalError || validateFull == true && analitico != null)
                {
                    //validacion de Adefa
                    //debe existir una preconfiguracion en Adefas para procesarlo
                    string yearDocument = comprobante.comprobante.Fecha.Substring(0, 4);
                    string yearCurrent = DateTime.Now.Year.ToString();
                    var resultRecuperado = await _adefasRepository.GetValidaPeriodoAdefaAsync(claveOrganismo, analitico.Ejercicio, comprobante.comprobante.Fecha.Substring(0, 10));
                    if (resultRecuperado.Status == HttpStatusCode.OK)
                    {
                        bool r = false;
                        if (resultRecuperado != null)
                        {
                            r = resultRecuperado.Data.valido;
                        }
                        if (r == false)
                        {
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20620", "20620", sourceDocument);
                            existsCriticalError = true;
                        }
                    }
                }

                if (!existsCriticalError || validateFull == true && analitico != null)
                {
                    await _bitacoraRepository.BitacoraInvoiceAP(comprobante.IdDocumento, Guid.Parse(comprobante.DocumentoBEId), "80", User, processBitacora);

                    var invoiceDto = setInvoiceDto(comprobante, analitico, "I", true, false, User.UserID, comprobante.ViaPago);
                    DataResult<InvoiceValidateDto> idto = new DataResult<InvoiceValidateDto>();
                    idto.User = User;
                    idto.Data = setInvoiceValidateDto(invoiceDto, comprobante.comprobante, claveOrganismo, comprobante.IdDocumento, validationErrors);
                    var validationInvoice = await _cFDIRepository.ValidateInvoiceAP(idto, comprobante.IdDocumento, comprobante.IdDocumento, comprobante.comprobante, comprobante.ComprobanteOriginal);
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
                    await _bitacoraRepository.BitacoraInvoiceAP(comprobante.IdDocumento, Guid.Parse(comprobante.DocumentoBEId), "90", User, processBitacora);
                }

                if (!existsCriticalError || validateFull == true && analitico != null)
                {
                    if (allowDuplicated == false)
                    {
                        await _bitacoraRepository.BitacoraInvoiceAP(comprobante.IdDocumento, Guid.Parse(comprobante.DocumentoBEId), "400", User, processBitacora);
                        var respUUID = await _facturaElectronicaRepository.GetInvoiceByUUID(comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID, comprobante.comprobante.Folio);
                        if (respUUID.Data != null)
                        {
                            comprobante.Status = -1;
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20422", "20422", sourceDocument);
                            existsCriticalError = true;
                        }

                        await _bitacoraRepository.BitacoraInvoiceAP(comprobante.IdDocumento, Guid.Parse(comprobante.DocumentoBEId), "410", User, processBitacora);

                    }
                }

                if (!existsCriticalError || validateFull == true && analitico != null)
                {
                    if (validateSAT)
                    {
                        await _bitacoraRepository.BitacoraInvoiceAP(comprobante.IdDocumento, Guid.Parse(comprobante.DocumentoBEId), "420", User, processBitacora);
                        SATResultDto sATResultDto = await _cFDIRepository.setSATResult(comprobante.comprobante.Emisor.Rfc, receptorRFC, comprobante.comprobante.Total.ToString(), comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID, validationErrors, errorCatalog, existsCriticalError, sourceDocument);
                        existsCriticalError = sATResultDto.existsCriticalError;
                        validationErrors = sATResultDto.validationErrors;

                        await _bitacoraRepository.BitacoraInvoiceAP(comprobante.IdDocumento, Guid.Parse(comprobante.DocumentoBEId), "430", User, processBitacora);
                    }
                }

                if (validateVersion)
                {
                    List<string> validVersions = await _cFDIRepository.GetValidVersions();
                    if (validVersions.Count > 0)
                    {
                        var x = validVersions.Where(x => x == comprobante.comprobante.Version).ToList();
                        if (x == null || x.Count == 0)
                        {
                            validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "20900", "20900", sourceDocument);
                            existsCriticalError = true;
                        }
                    }
                }

                int status = -10;
                if (validationErrors.Count == 0)
                {
                    var r = await processInvoice(comprobante, analitico, errorCatalog, validationErrors, "I", User, sourceDocument, comprobante.IdDocumento, Guid.Parse(comprobante.DocumentoBEId), comprobante.ViaPago);
                    status = r.status;
                    validationErrors = r.validationErrors;
                    invoiceIdResult = r.invoiceDto.InvoiceId;
                }

                if (!string.IsNullOrEmpty(comprobante.IdDocumento))
                {
                    if (validationErrors.Count > 0)
                    {
                        BitacoraInvoiceErrorInsertDto bie = new BitacoraInvoiceErrorInsertDto();
                        bie.reception = comprobante.IdDocumento;
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
                        await _cFDIRepository.setInitialRequestRejected(comprobante.IdDocumento, User.Email, comprobante.comprobante.Emisor.Rfc, receptorRFC);
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
                        biee.reception = comprobante.IdDocumento;
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
            if (existedException)
                comprobante.validationErrors = validationErrors;
            return invoiceResultDto;
        }

        //registro del invoice en la tabla
        public async Task<InvoiceProcessDto> processInvoice(InvoiceResultDto comprobante, AnaliticoPagoDto analitico, IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, string tipoComprobante, UsersDto User, string documento, string IdAnaliticoPago, Guid DocumentoBEId, string ViaPago)
        {
            bool existsCriticalError = false;
            string sapError = "";
            InvoiceProcessDto invoiceProcessDto = new InvoiceProcessDto();
            invoiceProcessDto.status = 0;
            invoiceProcessDto.validationErrors = validationErrors;

            InvoiceDto invoiceDto = setInvoiceDto(comprobante, analitico, tipoComprobante, false, false, User.UserID, ViaPago);

            //llamada a la api
            await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, DocumentoBEId, "180", User, processBitacora);
            CXPDto cXP = setApiCXPDto(comprobante, analitico, tipoComprobante, false, false, User.Token);
            var respApiCXP = await _sapPIRepository.PostCxP(cXP);
            await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, DocumentoBEId, "190", User, processBitacora);
            string uuid = comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID.ToString();

            CXPResultDto cXPResultDto = _cFDIRepository.setCxPResult(respApiCXP.Status, respApiCXP.Data, uuid, errorCatalog, validationErrors, documento, existsCriticalError);
            invoiceDto.SapDocument = cXPResultDto.sapDocument;
            sapError = cXPResultDto.sapError;
            validationErrors = cXPResultDto.validationErrors;
            existsCriticalError = cXPResultDto.existsCriticalError;

            //definir el status como En Proceso
            invoiceProcessDto.status = 0;

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
            iCxp.UserId = invoiceDto.UserId;

            //notas de credito
            List<InvoiceNotaCreditoDto> lrNC = new List<InvoiceNotaCreditoDto>() { };
            List<string> lrNCXML = new List<string>() { };

            await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, DocumentoBEId, "160", User, processBitacora);
            InvoiceSaveGralDto invoiceSaveGralDto = new InvoiceSaveGralDto();
            invoiceSaveGralDto.invoiceDto = invoiceDto;
            invoiceSaveGralDto.invoiceXML = comprobante.OriginalXML;
            invoiceSaveGralDto.invoiceCxPDto = iCxp;
            invoiceSaveGralDto.invoiceNotasCreditoDto = lrNC;
            invoiceSaveGralDto.InvoiceNotasCreditoXML = lrNCXML;
            var respAdd = await _facturaElectronicaRepository.SaveInvoiceGral(invoiceSaveGralDto);
            await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, DocumentoBEId, "170", User, processBitacora);
            if (respAdd.Status != HttpStatusCode.OK || respAdd.Data == null)
            {
                validationErrors = _cFDIRepository.addError(false, errorCatalog, validationErrors, "60003", "60003", documento);
                existsCriticalError = true;
            }
            else
            {
                invoiceProcessDto.invoiceDto = invoiceDto;
                invoiceProcessDto.invoiceDto.InvoiceId = respAdd.Data.invoiceDto.InvoiceId;
            }

            if (sapError != "")
            {
                await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, DocumentoBEId, sapError, User, processBitacora);
            }
            await _bitacoraRepository.BitacoraInvoiceAP(IdAnaliticoPago, DocumentoBEId, "330", User, processBitacora);


            if (!existsCriticalError)
            {
                invoiceProcessDto.status = 1;
            }

            invoiceProcessDto.validationErrors = validationErrors;
            return invoiceProcessDto;
        }

        //punto de entrada desde controller
        public async Task<InvoiceResultDto> SaveInvoice(InvoiceResultDto comprobante)
        {


            if (comprobante.comprobante == null)
                return null;

            var result = await processSaveInvoice(comprobante);

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
                var validacionError = await _correoRepository.NotificacionFacturaAPEmailAsync(comprobante.IdDocumento, validationErrors, comprobante.User, Guid.Parse(comprobante.DocumentoBEId), comprobante.User.Email, processBitacora, "CFDIAPRepository", "SaveInvoice");
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
    }
}
