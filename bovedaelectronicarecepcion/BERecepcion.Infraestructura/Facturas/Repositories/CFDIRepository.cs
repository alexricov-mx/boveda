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
using System.Text.RegularExpressions;
using Serilog;
using System.Xml.Serialization;
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
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Admin.Dto;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class CFDIRepository : BaseSQLServerSqlRepository, ICFDIRepository
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
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICFDIValidationRepository _cFDIValidationRepository;
        private bool processBitacora = false;

        public CFDIRepository(
            string cnnString
            , IConfiguration configuration
            , IBitacoraRepository bitacoraRepository
            , ISAPPIRepository sapPIRepository
            , IFacturaElectronicaRepository facturaElectronicaRepository
            , IAdefasRepository adefasRepository
            , ISATRepository sATRepository
            , ICorreoRepository correoRepository
            //, IUsuariosRepository usuariosRepository
            , IAnaliticoPagoRepository analiticoPagoRepository
            , IInvoiceRepository invoiceRepository
            , ICFDIValidationRepository cFDIValidationRepository) 
            : base(cnnString)
        {
            _configuration = configuration;
            _bitacoraRepository = bitacoraRepository;
            _sapPIRepository = sapPIRepository;
            _facturaElectronicaRepository = facturaElectronicaRepository;
            _adefasRepository = adefasRepository;
            _sapPIRepository = sapPIRepository;
            _sATRepository = sATRepository;
            _correoRepository = correoRepository;
            //_usuariosRepository = usuariosRepository;
            _analiticoPagoRepository = analiticoPagoRepository;
            _invoiceRepository = invoiceRepository;
            _cFDIValidationRepository = cFDIValidationRepository;
        }

        public async Task<DataResult<IEnumerable<ValidationError>>> GetValidationErrorCatalog()
        {
            DataResult<IEnumerable<ValidationError>> resultItem = new DataResult<IEnumerable<ValidationError>>
            {
                Status = HttpStatusCode.OK,
                Message = "GetValidationErrorCatalog"
            };

            try
            {
                DynamicParameters par = new DynamicParameters();

                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryMultipleAsync(sql: "SP_cfdi_getvalidationerrorcatalog", param: par, commandType: CommandType.StoredProcedure);
                    var resultData = await result.ReadAsync<ValidationError>();

                    List<ValidationError> dato = new List<ValidationError>();
                    foreach (var f in resultData)
                    {
                        ValidationError m = new ValidationError();
                        m.activo = f.activo;
                        m.clave = f.clave;
                        m.descripcion = f.descripcion;
                        m.esTerminal = f.esTerminal;
                        m.id = f.id;
                        dato.Add(m);
                    }

                    resultItem.Data = dato;

                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public async Task<string> GetOrganismRFC(string organismId)
        {
            string rfc = "";

            try
            {
                DynamicParameters par = new DynamicParameters();
                par.Add("@organismId", organismId);

                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    rfc = await db.QueryFirstAsync<string>(sql: "SP_CFDI_GetOrganismRFC", param: par, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                rfc = "";
                Log.Error(ex.Message);
            }

            return rfc;
        }

        public async Task<List<string>> GetValidVersions()
        {
            List<string> result = new List<string>();

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    var r = await db.QueryAsync<string>(sql: "SP_CFDI_GetValidVersions", commandType: CommandType.StoredProcedure);
                    if(r!=null)
                    {
                        foreach(var item in r)
                        {
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Clear();
                Log.Error(ex.Message);
            }

            return result;
        }

        public List<ValidationError> addError(bool esNotaCredito, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, string clave, string claveNC, string documento)
        {
            return _cFDIValidationRepository.addError(esNotaCredito, catalogErrors, validationErrors, clave, claveNC, documento);
        }

        public List<int> setCopadeNC(string copadeNotasCredito, ComprobanteBE notaCredito, out CopadeDto copade, List<int> processed)
        {
            copade = null;
            List<int> _processed = processed;

            try
            {
                CopadeNotasCredito cnc = JsonConvert.DeserializeObject<CopadeNotasCredito>(copadeNotasCredito);
                if (cnc != null)
                {
                    int iNotaCredito = -1;
                    foreach (var cncc in cnc.notaCredito)
                    {
                        iNotaCredito++;
                        if (notaCredito.Conceptos.Concepto.Count >= 1)
                        {
                            foreach (var cn in notaCredito.Conceptos.Concepto)
                            {
                                if (notaCredito.SubTotal == Convert.ToDouble(cncc.Importe) && Convert.ToDouble(cn.Cantidad) == Convert.ToDouble(cncc.Cantidad))
                                {
                                    bool found = false;
                                    for (int i = 0; i < _processed.Count; i++)
                                    {
                                        if (_processed[i] == iNotaCredito) found = true;
                                    }
                                    if (!found)
                                    {
                                        copade = new CopadeDto();
                                        copade.Total = cncc.Total;
                                        copade.Iva = cncc.Iva;
                                        CopadeComprobanteConceptos c = new CopadeComprobanteConceptos();
                                        c.cantidad = cncc.Cantidad;
                                        c.importe = cncc.Total.ToString();
                                        c.valorUnitario = cncc.Total.ToString();
                                        
                                        List<CopadeComprobanteConceptos> lc = new List<CopadeComprobanteConceptos>();
                                        lc.Add(c);
                                        copade.vPreFactura = new CopadePreFactura();
                                        copade.vPreFactura.comprobante = new CopadePreFacturaComprobante();
                                        copade.vPreFactura.comprobante.conceptos = lc;

                                        _processed.Add(iNotaCredito);

                                        return _processed;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                copade = null;
                Log.Error(ex.Message);
            }

            return _processed;
        }

        public InvoiceDto setInvoiceDto(ComprobanteDto comprobante, CopadeDto copade, string tipoComprobante, bool isElectronicReception, bool isCopade, Guid UserId)
        {
            InvoiceDto dto = new InvoiceDto();

            double diferencialAbono, diferencialCargo;
            diferencialAbono = diferencialCargo = 0;
            if (string.IsNullOrEmpty(copade.Total)) copade.Total = "0";
            if (string.IsNullOrEmpty(copade.Subtotal)) copade.Subtotal = "0";

            if (Convert.ToDouble(comprobante.comprobante.Total) < Convert.ToDouble(copade.Total)) diferencialCargo = Math.Round(Math.Abs(Convert.ToDouble(comprobante.comprobante.Total) - Convert.ToDouble(copade.Total)), 2);
            if (Convert.ToDouble(comprobante.comprobante.Total) > Convert.ToDouble(copade.Total)) diferencialAbono = Math.Round(Math.Abs(Convert.ToDouble(comprobante.comprobante.Total) - Convert.ToDouble(copade.Total)), 2);

            dto.UserId = UserId;
            dto.Assignment = comprobante.comprobante.Addenda.Addenda_Pemex.VUREGION;
            dto.OriginalXML = comprobante.comprobante.Cfdi;
            dto.CompensationDocument = "";
            dto.DiferencialAbono = diferencialAbono.ToString();
            dto.DiferencialCargo = diferencialCargo.ToString();
            dto.DocumentoBEId = copade.CopadeID;
            dto.ElectronicReception = isElectronicReception ? "E" : "D";
            dto.EmailProvider = "";
            dto.EmailProviderSendDate = DateTime.Now;
            dto.Estatus = "0";
            dto.FechaEmision = null;
            dto.Folio = comprobante.comprobante.Folio;
            dto.ImporteOriginal = copade.Total;
            dto.InvoiceDate = convertToDate(comprobante.comprobante.Fecha);
            dto.InvoiceId = Guid.Empty;
            dto.IsCopade = isCopade;
            dto.LastStatus = "0";
            dto.LastStatusDate = DateTime.Now;
            dto.ReceptionDate = DateTime.Now;
            dto.RutaArchivo = comprobante.rutaArchivo;
            dto.SapDocument = "";
            dto.Serie = comprobante.comprobante.Serie;
            dto.Subtotal = comprobante.comprobante.SubTotal.ToString();
            dto.TipoComprobante = tipoComprobante;
            dto.Total = comprobante.comprobante.Total.ToString();
            dto.TotalImpuestosRetenidos = comprobante.comprobante.Impuestos.TotalImpuestosRetenidos.ToString();
            dto.TotalImpuestosTrasladados = comprobante.comprobante.Impuestos.TotalImpuestosTrasladados.ToString();
            dto.Uuid = comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID;
            dto.ViaPago = comprobante.comprobante.Addenda.Addenda_Pemex.VUREGION;
            dto.CFDIVersion = comprobante.comprobante.Version;

            return dto;
        }

        public CXPDto setApiCXPDto(ComprobanteDto comprobante, CopadeDto copade, string tipoComprobante, bool isElectronicReception, bool isCopade, string token)
        {
            CXPDto dto = new CXPDto();

            double diferencialAbono, diferencialCargo;
            diferencialAbono = diferencialCargo = 0;
            if (string.IsNullOrEmpty(copade.Total)) copade.Total = "0";
            if (string.IsNullOrEmpty(copade.Subtotal)) copade.Subtotal = "0";

            if (Convert.ToDouble(comprobante.comprobante.Total) < Convert.ToDouble(copade.Total)) diferencialCargo = Math.Round(Math.Abs(Convert.ToDouble(comprobante.comprobante.Total) - Convert.ToDouble(copade.Total)), 2);
            if (Convert.ToDouble(comprobante.comprobante.Total) > Convert.ToDouble(copade.Total)) diferencialAbono = Math.Round(Math.Abs(Convert.ToDouble(comprobante.comprobante.Total) - Convert.ToDouble(copade.Total)), 2);

            dto.CentroGestor = "";
            dto.Cliente = "";
            dto.ContratoVigente = "";
            dto.Cliente = "";
            dto.DiferencialAbono = diferencialAbono.ToString();
            dto.DiferencialCargo = diferencialCargo.ToString();
            dto.DocumentoSAP = "";
            dto.Ejercicio = copade.Exercise;
            dto.Entrada = copade.Reception;
            dto.Factura = comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID.ToString();
            dto.FechaEmision = null;
            dto.FechaFactura = comprobante.comprobante.Fecha;
            dto.FechaRecep = DateTime.Now.ToString("yyyy-MM-dd");
            dto.Id_Analitico = "";
            dto.ImporteFactura = comprobante.comprobante.Total.ToString();
            dto.ImporteOriginal = copade.Total;
            dto.Mensaje = "";
            dto.OrdenSap = "";
            dto.Organismo = comprobante.claveOrganismo;
            dto.Res = "";
            dto.Usuario = token;
            dto.ViaPago = comprobante.comprobante.Addenda.Addenda_Pemex.VUREGION;

            return dto;

        }

        public InvoiceNotaCreditoDto setInvoiceNotaCreditoDto(ComprobanteBE comprobante, Guid DocumentoBEId, Guid InvoiceId, bool isCopade, double amount, double iva, double total)
        {
            InvoiceNotaCreditoDto iNC = new InvoiceNotaCreditoDto();
            iNC.Amount = amount.ToString();
            iNC.OriginalXML = comprobante.Cfdi;
            iNC.DocumentoBEId = DocumentoBEId;
            iNC.InvoiceId = InvoiceId;
            iNC.IsCopade = isCopade;
            iNC.Iva = iva.ToString();
            iNC.Total = total.ToString();
            iNC.Uuid = comprobante.Complemento.TimbreFiscalDigital.UUID;
            iNC.Serie = comprobante.Serie;
            iNC.Folio = comprobante.Folio;
            iNC.NotaCreditoDate = convertToDate(comprobante.Fecha);
            iNC.CFDIVersion = comprobante.Version;
            return iNC;
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

        //registro en la base de datos
        public async Task<InvoiceProcessDto> processInvoice(ComprobanteDto comprobante, CopadeDto copade, IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, string tipoComprobante, UsersDto User, string documento)
        {
            bool existsCriticalError = false;
            string sapError = "";
            InvoiceProcessDto invoiceProcessDto = new InvoiceProcessDto();
            invoiceProcessDto.status = 0;
            invoiceProcessDto.validationErrors = validationErrors;

            InvoiceDto invoiceDto = setInvoiceDto(comprobante, copade, tipoComprobante, !comprobante.esDocumental, comprobante.esCopade, User.UserID);

            if (validationErrors.Count == 0)
            {
                //llamada a la api
                await _bitacoraRepository.BitacoraInvoice(copade.Reception, "180", User, processBitacora);
                CXPDto cXP = setApiCXPDto(comprobante, copade, tipoComprobante, !comprobante.esDocumental, comprobante.esCopade, User.Token);
                var respApiCXP = await _sapPIRepository.PostCxP(cXP);
                await _bitacoraRepository.BitacoraInvoice(copade.Reception, "190", User, processBitacora);
                string uuid = comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID.ToString();

                CXPResultDto cXPResultDto = setCxPResult(respApiCXP.Status, respApiCXP.Data, uuid, errorCatalog, validationErrors, documento, existsCriticalError);
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
                if (comprobante.notasCredito != null)
                {
                    List<int> processed = new List<int>() { };
                    foreach (var nc in comprobante.notasCredito)
                    {
                        CopadeDto copadeNC = null;
                        processed = setCopadeNC(copade.NotasCredito, nc, out copadeNC, processed);
                        if (copadeNC != null)
                        {
                            InvoiceNotaCreditoDto iNC = setInvoiceNotaCreditoDto(nc, invoiceDto.DocumentoBEId, Guid.Empty, comprobante.esCopade, Convert.ToDouble(copadeNC.Total), Convert.ToDouble(copadeNC.Iva), nc.Total);
                            lrNC.Add(iNC);
                            if (comprobante.notasCreditoCFDIXML != null)
                            {
                                foreach (var ncc in comprobante.notasCreditoCFDIXML)
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

                await _bitacoraRepository.BitacoraInvoice(copade.Reception, "160", User, processBitacora);
                InvoiceSaveGralDto invoiceSaveGralDto = new InvoiceSaveGralDto();
                invoiceSaveGralDto.invoiceDto = invoiceDto;
                invoiceSaveGralDto.invoiceXML = comprobante.OriginalXml;
                invoiceSaveGralDto.invoiceCxPDto = iCxp;
                invoiceSaveGralDto.invoiceNotasCreditoDto = lrNC;
                invoiceSaveGralDto.InvoiceNotasCreditoXML = lrNCXML;
                var respAdd = await _facturaElectronicaRepository.SaveInvoiceGral(invoiceSaveGralDto);
                await _bitacoraRepository.BitacoraInvoice(copade.Reception, "170", User, processBitacora);
                if (respAdd.Status != HttpStatusCode.OK || respAdd.Data == null)
                {
                    addError(false, errorCatalog, validationErrors, "60003", "60003", documento);
                    existsCriticalError = true;
                }
                else
                {
                    invoiceProcessDto.invoiceDto = invoiceDto;
                    invoiceProcessDto.invoiceDto.InvoiceId = respAdd.Data.invoiceDto.InvoiceId;
                }

            }

            if (sapError != "")
            {
                await _bitacoraRepository.BitacoraInvoice(copade.Reception, sapError, User, processBitacora);
            }
            await _bitacoraRepository.BitacoraInvoice(copade.Reception, "330", User, processBitacora);

            if (!existsCriticalError)
            {
                invoiceProcessDto.status = 1;
            }

            invoiceProcessDto.validationErrors = validationErrors;
            return invoiceProcessDto;
        }

        public async Task<List<ValidationError>> ValidatePep(string claveOrganismo, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, string documento, bool esNotaCredito)
        {
            try
            {
                string fechaRecepcion = comprobanteBE.Fecha.Substring(0, 10);
                ValidacionCXPDto dtoMiro = new ValidacionCXPDto() { Contrato = copade.Contract, Fecha = fechaRecepcion, Organismo = claveOrganismo };
                var r = await _sapPIRepository.PostValidacionCxP(dtoMiro);
                if (r.Data.Status == false)
                {
                    validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10500", "20500", documento);
                    if (!string.IsNullOrEmpty(r.Data.Mensaje))
                    {
                        validationErrors[validationErrors.Count - 1].descripcion = r.Data.Mensaje;
                        validationErrors[validationErrors.Count - 1].mensaje = r.Data.Mensaje;
                    }
                }
            }
            catch (Exception ex)
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10500", "20500", documento);
                Log.Error(ex.Message);
            }

            return validationErrors;
        }

        public async Task<List<ValidationError>> ValidatePepAP(string claveOrganismo, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, AnaliticoPagoDto analitico, string documento, bool esNotaCredito)
        {
            try
            {
                string fechaRecepcion = comprobanteBE.Fecha.Substring(0, 10);
                ValidacionCXPDto dtoMiro = new ValidacionCXPDto() { Contrato = analitico.Contrato, Fecha = fechaRecepcion, Organismo = claveOrganismo };
                var r = await _sapPIRepository.PostValidacionCxP(dtoMiro);
                if (r.Data.Status == false)
                {
                    validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10500", "20500", documento);
                    if (!string.IsNullOrEmpty(r.Data.Mensaje))
                    {
                        validationErrors[validationErrors.Count - 1].descripcion = r.Data.Mensaje;
                        validationErrors[validationErrors.Count - 1].mensaje = r.Data.Mensaje;
                    }
                }
            }
            catch (Exception ex)
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10500", "20500", documento);
                Log.Error(ex.Message);
            }

            return validationErrors;
        }

        //punto de entrada desde controller
        public async Task<ComprobanteDto> SaveInvoice(ComprobanteDto comprobante)
        {
            if (String.IsNullOrEmpty(comprobante.ComprobanteBEString))
                return null;

            comprobante.comprobante = JsonConvert.DeserializeObject<ComprobanteBE>(comprobante.ComprobanteBEString);

            if (comprobante.ComprobanteBEString == null)
                return null;

            string reception = "";
            var result = await processSaveInvoice(comprobante);

            List<ValidationError> validationErrors = result.validationErrors.ToList();

            if (comprobante.comprobante.Addenda != null)
            {
                if (comprobante.comprobante.Addenda.Addenda_Pemex != null)
                {
                    reception = comprobante.comprobante.Addenda.Addenda_Pemex.ENTRADA.ToString() + "_CFDI_" + comprobante.comprobante.Version;
                }
            }

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
                var validacionError = await _correoRepository.NotificacionFacturaEmailAsync(reception, validationErrors, comprobante.User, processBitacora, "CFDIRepository", "SaveInvoice");
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

        //punto de entrada desde controller
        public async Task<IEnumerable<ComprobanteDto>> SaveInvoiceMultiple(IEnumerable<ComprobanteDto> comprobantes)
        {
            List<ValidationError> finalValidationErrors = null;
            List<ComprobanteDto> comprobantesResult = new List<ComprobanteDto>();
            UsersDto User = null;
            string reception = "";

            foreach (var comprobante in comprobantes)
            {
                if (User == null) User = comprobante.User;
                var result = await processSaveInvoice(comprobante);
                if (result != null)
                {
                    comprobantesResult.Add(result);
                    IEnumerable<ValidationError> validationErrors = result.validationErrors;
                    if (finalValidationErrors == null)
                    {
                        finalValidationErrors = validationErrors.ToList();
                    }
                    else
                    {
                        foreach (var item in validationErrors)
                        {
                            finalValidationErrors.Add(item);
                        }
                    }
                    if (comprobante.comprobante.Addenda != null)
                    {
                        if (comprobante.comprobante.Addenda.Addenda_Pemex != null)
                        {
                            reception = reception + comprobante.comprobante.Addenda.Addenda_Pemex.ENTRADA.ToString() + "_CFDI_" + comprobante.comprobante.Version + ",";
                        }
                    }
                }
            }


            try
            {
                bool processBitacora = true;
                if (!string.IsNullOrEmpty(reception))
                {
                    reception = reception.Substring(0, reception.Length - 1);
                }
                try
                {
                    processBitacora = Convert.ToBoolean(_configuration["BitacoraInvoice:Habilitado"]);
                }
                catch (Exception exx)
                {
                    processBitacora = true;
                    Log.Error(exx.Message);
                }
                var validacionError = await _correoRepository.NotificacionFacturaEmailAsync(reception, finalValidationErrors, User, processBitacora, "CFDIRepository", "SaveInvoiceMultiple");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return comprobantesResult;
        }

        public async Task<ComprobanteDto> processSaveInvoice(ComprobanteDto comprobante)
        {
            if (comprobante.comprobante == null)
                return null;

            IEnumerable<ValidationError> errorCatalog = new List<ValidationError>() { };
            List<ValidationError> validationErrors = new List<ValidationError>();
            CopadeDto copade = null;
            bool existsCriticalError = false;
            bool allowDuplicated = false;
            bool validateSAT = true;
            bool validatePEP = true;
            bool validateFull = true;
            bool validateVersion = false;
            string reception = "";
            string exercise = "0";
            bool existedException = false;
            string exceptionMessage = "";
            UsersDto User = comprobante.User;
            string sourceDocument = "";

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
                validateFull = true;
                validateVersion = false;
                Log.Error(exx.Message);
            }

            try
            {
                string claveOrganismo, receptorRFC;
                claveOrganismo = reception = receptorRFC = "";

                claveOrganismo = comprobante.claveOrganismo;

                if (!existsCriticalError)
                {
                    if (comprobante.comprobante == null)
                    {
                        comprobante = new ComprobanteDto();
                        comprobante.status = -1;
                        addError(false, errorCatalog, validationErrors, "00000", "00000", sourceDocument);
                        existsCriticalError = true;
                    }
                }

                await logInitialRequest(comprobante);

                if (!existsCriticalError)
                {
                    if (comprobante.comprobante.Addenda != null)
                    {
                        if (comprobante.comprobante.Addenda.Addenda_Pemex != null)
                        {
                            reception = comprobante.comprobante.Addenda.Addenda_Pemex.ENTRADA.ToString();
                            exercise = comprobante.comprobante.Addenda.Addenda_Pemex.EJERCICIO;
                        }
                        else
                        {
                            addError(false, errorCatalog, validationErrors, "90002", "90002", sourceDocument);
                            existsCriticalError = true;
                        }
                    }
                    else
                    {
                        addError(false, errorCatalog, validationErrors, "90002", "90002", sourceDocument);
                        existsCriticalError = true;
                    }
                    sourceDocument = setSourceDocument(comprobante.comprobante, reception);
                }

                if (!existsCriticalError)
                {
                    await _bitacoraRepository.BitacoraInvoice(reception, "60", User, processBitacora);
                    var rslt = await GetValidationErrorCatalog();
                    if (rslt.Status != HttpStatusCode.OK)
                    {
                        comprobante.status = -1;
                        addError(false, errorCatalog, validationErrors, "90000", "90000", sourceDocument);
                        existsCriticalError = true;
                    }
                    else
                    {
                        errorCatalog = rslt.Data;
                        if (errorCatalog == null)
                        {
                            comprobante.status = -1;
                            addError(false, errorCatalog, validationErrors, "90001", "90001", sourceDocument);
                            existsCriticalError = true;
                        }
                    }
                    await _bitacoraRepository.BitacoraInvoice(reception, "70", User, processBitacora);
                }

                if (!existsCriticalError)
                {
                    await _bitacoraRepository.BitacoraInvoice(reception, "40", User, processBitacora);
                    DataResult<CopadeDto> cpd = new DataResult<CopadeDto>();
                    PICopadeRequestDto pi = new PICopadeRequestDto() { Clave = claveOrganismo, Reception = reception, Exercise = exercise };
                    cpd = await _sapPIRepository.RecuperaCopadeBDAsync(pi);

                    if (cpd.Status != HttpStatusCode.OK)
                    {
                        comprobante.status = -1;
                        addError(false, errorCatalog, validationErrors, "12000", "12000", sourceDocument);
                        existsCriticalError = true;
                    }
                    else
                    {
                        if (cpd.Data == null)
                        {
                            comprobante.status = -1;
                            addError(false, errorCatalog, validationErrors, "12000", "12000", sourceDocument);
                            existsCriticalError = true;
                        }
                        else
                        {
                            copade = cpd.Data;
                            if (copade == null)
                            {
                                comprobante.status = -1;
                                addError(false, errorCatalog, validationErrors, "12000", "12000", sourceDocument);
                                existsCriticalError = true;
                            }
                            else
                            {
                                if (copade.IsCancel == true)
                                {
                                    comprobante.status = -1;
                                    addError(false, errorCatalog, validationErrors, "12001", "12001", sourceDocument);
                                    existsCriticalError = true;
                                }
                                else
                                {
                                    if (copade.IsFullSigned == false)
                                    {
                                        comprobante.status = -1;
                                        addError(false, errorCatalog, validationErrors, "12002", "12002", sourceDocument);
                                        existsCriticalError = true;
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(copade.Reception))
                                        {
                                            comprobante.status = -1;
                                            addError(false, errorCatalog, validationErrors, "12000", "12000", sourceDocument);
                                            existsCriticalError = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    await _bitacoraRepository.BitacoraInvoice(reception, "50", User, processBitacora);
                }

                if (!existsCriticalError)
                {
                    Guid organismId = copade.OrganismID;
                    receptorRFC = await GetOrganismRFC(organismId.ToString());
                }

                if (validateVersion)
                {
                    if (comprobante.notasCredito != null)
                    {
                        foreach (var nc in comprobante.notasCredito)
                        {
                            if (comprobante.comprobante.Version != nc.Version)
                            {
                                comprobante.status = -1;
                                addError(false, errorCatalog, validationErrors, "20800", "20800", sourceDocument);
                                existsCriticalError = true;
                            }
                        }
                    }

                    List<string> validVersions = await GetValidVersions();
                    if(validVersions.Count > 0)
                    {
                        var x = validVersions.Where(x => x == comprobante.comprobante.Version).ToList();
                        if(x == null || x.Count==0)
                        {
                            comprobante.status = -1;
                            addError(false, errorCatalog, validationErrors, "20900", "20900", sourceDocument);
                            existsCriticalError = true;
                        }
                        foreach (var nc in comprobante.notasCredito)
                        {
                            var y = validVersions.Where(x => x == nc.Version);
                            if (y == null)
                            {
                                comprobante.status = -1;
                                addError(false, errorCatalog, validationErrors, "20800", "20800", sourceDocument);
                                existsCriticalError = true;
                            }
                        }
                    }
                }

                if (!existsCriticalError)
                {
                    //validacion de Adefa
                    //debe existir una preconfiguracion en Adefas para procesarlo
                    string yearDocument = comprobante.comprobante.Fecha.Substring(0, 4);
                    string yearCurrent = DateTime.Now.Year.ToString();
                    var resultRecuperado = await _adefasRepository.GetValidaPeriodoAdefaAsync(claveOrganismo, copade.Exercise, comprobante.comprobante.Fecha.Substring(0, 10));
                    if (resultRecuperado.Status == HttpStatusCode.OK)
                    {
                        bool r = false;
                        if (resultRecuperado.Data != null)
                        {
                            r = resultRecuperado.Data.valido;
                        }
                        if (r == false)
                        {
                            validationErrors = addError(false, errorCatalog, validationErrors, "20620", "20620", sourceDocument);
                            existsCriticalError = true;
                        }
                    }
                }

                if (!existsCriticalError || validateFull && copade != null)
                {
                    await _bitacoraRepository.BitacoraInvoice(reception, "80", User, processBitacora);
                    validationErrors = await validateDetailComprobante(claveOrganismo, receptorRFC, errorCatalog, validationErrors, comprobante.comprobante, copade, sourceDocument, false, comprobante.ComprobanteOriginal);
                    await _bitacoraRepository.BitacoraInvoice(reception, "90", User, processBitacora);
                    if (comprobante.notasCredito != null)
                    {
                        int qtyNotasCredito = 0;
                        List<int> processed = new List<int>() { };
                        int pos = 0;
                        foreach (var nc in comprobante.notasCredito)
                        {
                            qtyNotasCredito++;
                            string sourceDocumentNC = setSourceDocumentNC(nc, reception);
                            await _bitacoraRepository.BitacoraInvoice(reception, "120", User, processBitacora);
                            if (nc.CfdiRelacionados != null)
                            {
                                if (nc.CfdiRelacionados.CfdiRelacionado != null)
                                {
                                    if (!string.IsNullOrEmpty(nc.CfdiRelacionados.CfdiRelacionado.UUID))
                                    {
                                        if (comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID != Guid.Empty)
                                        {
                                            if (nc.CfdiRelacionados.CfdiRelacionado.UUID.ToUpper() != comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID.ToString().ToUpper())
                                            {
                                                comprobante.status = -1;
                                                addError(false, errorCatalog, validationErrors, "20700", "20700", sourceDocumentNC);
                                                existsCriticalError = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(copade.NotasCredito))
                            {
                                comprobante.status = -1;
                                addError(false, errorCatalog, validationErrors, "12003", "12003", sourceDocument);
                                existsCriticalError = true;
                            }
                            else
                            {
                                CopadeDto copadeNC = null;
                                processed = setCopadeNC(copade.NotasCredito, nc, out copadeNC, processed);
                                if (copadeNC == null)
                                {
                                    comprobante.status = -1;
                                    addError(false, errorCatalog, validationErrors, "12004", "12004", sourceDocumentNC);
                                    existsCriticalError = true;
                                }
                                else
                                {
                                    validationErrors = await validateDetailNCComprobante(claveOrganismo, errorCatalog, validationErrors, nc, copadeNC, true, comprobante.comprobante.Emisor.Rfc, receptorRFC, comprobante.comprobante.Moneda, sourceDocumentNC);
                                }
                            }
                            pos++;
                            await _bitacoraRepository.BitacoraInvoice(reception, "130", User, processBitacora);
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(copade.NotasCredito))
                            {
                                CopadeNotasCredito cnc = JsonConvert.DeserializeObject<CopadeNotasCredito>(copade.NotasCredito);
                                if (cnc == null)
                                {
                                    comprobante.status = -1;
                                    addError(false, errorCatalog, validationErrors, "12006", "12006", sourceDocument);
                                    existsCriticalError = true;
                                }
                                else
                                {
                                    int qtyCopadeNotaCredito = 0;
                                    foreach (var cnct in cnc.notaCredito)
                                        qtyCopadeNotaCredito++;
                                    if (qtyCopadeNotaCredito != qtyNotasCredito)
                                    {
                                        comprobante.status = -1;
                                        addError(false, errorCatalog, validationErrors, "12006", "12006", sourceDocument);
                                        existsCriticalError = true;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            comprobante.status = -1;
                            addError(false, errorCatalog, validationErrors, "12006", "12006", sourceDocument);
                            existsCriticalError = true;
                            Log.Error(ex.Message);
                        }
                    }
                }

                if (!existsCriticalError || validateFull && copade != null)
                {
                    //PEP Valida vigencia de las Fuentes de Financiamiento
                    if (validatePEP)
                    {
                        if (claveOrganismo == "PEP")
                        {
                            validationErrors = await ValidatePep(claveOrganismo, errorCatalog, validationErrors, comprobante.comprobante, copade, sourceDocument, false);
                            var existError = validationErrors.Where(x => x.clave == "10500" || x.clave == "20500").FirstOrDefault();
                            if (existError != null) existsCriticalError = true;
                        }
                    }
                }

                if (!existsCriticalError || validateFull && copade != null)
                {
                    if (allowDuplicated == false)
                    {
                        await _bitacoraRepository.BitacoraInvoice(reception, "400", User, processBitacora);
                        var respUUID = await _facturaElectronicaRepository.GetInvoiceByUUID(comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID, comprobante.comprobante.Folio);
                        if (respUUID.Data != null)
                        {
                            comprobante.status = -1;
                            addError(false, errorCatalog, validationErrors, "20422", "20422", sourceDocument);
                            existsCriticalError = true;
                        }

                        await _bitacoraRepository.BitacoraInvoice(reception, "410", User, processBitacora);
                        if (comprobante.notasCredito != null)
                        {
                            foreach (var nc in comprobante.notasCredito)
                            {
                                string sourceDocumentNC = setSourceDocumentNC(nc, reception);
                                await _bitacoraRepository.BitacoraInvoice(reception, "400", User, processBitacora);
                                var respNCUUID = await _facturaElectronicaRepository.GetInvoiceNotaCreditoByUUID(nc.Complemento.TimbreFiscalDigital.UUID, nc.Folio);
                                if (respNCUUID.Data != null)
                                {
                                    comprobante.status = -1;
                                    addError(false, errorCatalog, validationErrors, "20423", "20423", sourceDocumentNC);
                                    existsCriticalError = true;
                                }
                                await _bitacoraRepository.BitacoraInvoice(reception, "410", User, processBitacora);
                            }
                        }
                    }
                }

                if (!existsCriticalError || validateFull && copade != null)
                {
                    if (validateSAT)
                    {
                        await _bitacoraRepository.BitacoraInvoice(reception, "420", User, processBitacora);
                        SATResultDto sATResultDto = await setSATResult(comprobante.comprobante.Emisor.Rfc, receptorRFC, comprobante.comprobante.Total.ToString(), comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID, validationErrors, errorCatalog, existsCriticalError, sourceDocument);
                        existsCriticalError = sATResultDto.existsCriticalError;
                        validationErrors = sATResultDto.validationErrors;
                        await _bitacoraRepository.BitacoraInvoice(reception, "430", User, processBitacora);
                    }
                }

                int status = -10;
                if (validationErrors.Count == 0)
                {
                    var r = await processInvoice(comprobante, copade, errorCatalog, validationErrors, "I", User, sourceDocument);
                    status = r.status;
                    validationErrors = r.validationErrors;
                }

                if (!string.IsNullOrEmpty(reception))
                {
                    if (validationErrors.Count > 0)
                    {
                        BitacoraInvoiceErrorInsertDto bie = new BitacoraInvoiceErrorInsertDto();
                        bie.reception = reception;
                        bie.medioProceso = 0;
                        bie.UserId = User.UserID.ToString();
                        bie.errors = validationErrors;

                        var respbie = await _bitacoraRepository.InsertaBitacoraInvoiceErrorAsync(bie);
                    }
                }

                comprobante.validationErrors = validationErrors;
                comprobante.copade = copade;
                int qtyCritical = validationErrors.Where(x => x.esTerminal == true).Count();
                if (qtyCritical > 0)
                {
                    var v = validationErrors.Where(x => x.clave == "60000").FirstOrDefault();
                    if (v == null)
                    {
                        comprobante.status = -1;
                        await setInitialRequestRejected(reception, User.Email, comprobante.comprobante.Emisor.Rfc, receptorRFC);
                    }
                }
                else
                {
                    comprobante.status = status;
                }

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
                        biee.reception = reception;
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

            comprobante.existedException = existedException;
            comprobante.exceptionMessage = exceptionMessage;
            if (existedException)
                comprobante.validationErrors = validationErrors;
            return comprobante;
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

        public async Task<bool> logInitialRequest(ComprobanteDto comprobante)
        {
            string reception = "";
            string user = "";
            string xml = "";
            bool esCopade, esDocumental;
            esCopade = esDocumental = true;
            List<string> ncxml = new List<string>() { };
            string claveOrganismo = "";

            if (comprobante != null)
            {
                if (comprobante.comprobante != null)
                {
                    xml = comprobante.OriginalXml;
                    claveOrganismo = comprobante.claveOrganismo;
                    esCopade = comprobante.esCopade;
                    esDocumental = comprobante.esDocumental;
                    if (comprobante.comprobante.Addenda != null)
                    {
                        if (comprobante.comprobante.Addenda.Addenda_Pemex != null)
                        {
                            reception = comprobante.comprobante.Addenda.Addenda_Pemex.ENTRADA.ToString();
                        }
                    }
                    if (comprobante.notasCredito != null)
                    {
                        foreach (var nc in comprobante.notasCredito)
                        {
                            if (comprobante.notasCreditoCFDIXML != null)
                            {
                                foreach (var ncxmlItem in comprobante.notasCreditoCFDIXML)
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
                }

                user = comprobante.User.UserID.ToString();
                claveOrganismo = comprobante.claveOrganismo;

                await _bitacoraRepository.logInitialRequest(claveOrganismo, reception, user, xml, ncxml, esCopade, esDocumental);

            }

            return true;
        }

        #region detalle validaciones
        public async Task<List<ValidationError>> validateDetailComprobante(string clave, string receptorRFC, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, string documento, bool esNotaCredito, string comprobanteOriginal)
        {
            return await _cFDIValidationRepository.validateDetailComprobante(clave, receptorRFC, catalogErrors, validationErrors, comprobanteBE, copade, documento, esNotaCredito, comprobanteOriginal);
        }
        public async Task<List<ValidationError>> validateDetailNCComprobante(string clave, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, bool esNotaCredito, string cfdiEmisorRFC, string cfdiReceptorRFC, string cfdiMoneda, string documento)
        {
            return await _cFDIValidationRepository.validateDetailNCComprobante(clave, catalogErrors, validationErrors, comprobanteBE, copade, esNotaCredito, cfdiEmisorRFC, cfdiReceptorRFC, cfdiMoneda, documento);
        }
        public async Task<List<ValidationError>> validateDetailComprobateAP(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, InvoiceValidateDto dto, AnaliticoPagoDto analitico, ComprobanteBE comprobante, string sourceDocument, bool esNotaCredito, string comprobanteOriginal)
        {
            return await _cFDIValidationRepository.validateDetailComprobateAP(errorCatalog, validationErrors, dto, analitico, comprobante, sourceDocument, esNotaCredito, comprobanteOriginal);
        }
        #endregion

        public async Task<DataResult<List<PendingInvoiceDto>>> GetPendingInvoice()
        {
            DataResult<List<PendingInvoiceDto>> resultItem = new DataResult<List<PendingInvoiceDto>>()
            {
                Status = HttpStatusCode.OK
            };
            try
            {
                List<PendingInvoiceDto> pl = new List<PendingInvoiceDto>() { };
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryAsync<PendingInvoiceDto>(sql: "SP_invoice_get_pending", param: null, commandType: CommandType.StoredProcedure);

                    foreach (var r in result)
                    {
                        PendingInvoiceDto p = new PendingInvoiceDto();
                        List<string> nc = new List<string>();
                        p.ClaveOrganismo = r.ClaveOrganismo;
                        p.EntryDate = r.EntryDate;
                        p.EsCopade = r.EsCopade;
                        p.EsDocumental = r.EsDocumental;
                        p.Id = r.Id;
                        p.NotasCredito = nc;
                        p.Reception = r.Reception;
                        p.UserId = r.UserId;
                        p.XmlContent = r.XmlContent;
                        p.tipo = r.tipo;
                        if (p.Id > 0)
                        {
                            if (!string.IsNullOrEmpty(p.XmlContent))
                            {
                                DynamicParameters par = new DynamicParameters();
                                par.Add("@Id", p.Id);
                                var rslt = await db.QueryAsync<string>(sql: "SP_invoice_get_pending_notacredito", param: par, commandType: CommandType.StoredProcedure);
                                foreach (var rr in rslt)
                                {
                                    p.NotasCredito.Add(rr);
                                }
                            }
                        }
                        pl.Add(p);
                    }

                    resultItem.Data = pl;
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                resultItem.Data = null;
            }

            return resultItem;
        }

        public async Task<DataResult<bool>> ReprocessInvoice()
        {
            DataResult<bool> result = new DataResult<bool>()
            {
                Status = HttpStatusCode.OK,
                Message = "Reproceso exitoso."
            };

            var inv = await GetPendingInvoice();
            var invoices = inv.Data;
            if (invoices != null)
            {
                var pendings = invoices.Where(x => x.tipo == 1).ToList();
                var pendings200 = invoices.Where(x => x.tipo == 2).ToList();
                if (pendings != null)
                {
                    await ReprocessInvoicePendings(pendings);
                }
                if (pendings200 != null)
                {
                    await ReprocessInvoicePendings200(pendings200);
                }
            }

            await ReprocessInvoiceEmail();

            result.Data = true;
            return result;
        }

        private void ReprocessInvoiceLists(List<PendingInvoiceDto> pendings, out List<ComprobanteDto> _comprobantes, out List<int> _errors)
        {
            _comprobantes = null;
            _errors = null;

            List<ComprobanteDto> comprobantes = new List<ComprobanteDto>();
            List<int> errors = new List<int>() { };

            XmlSerializer serializer = new XmlSerializer(typeof(ComprobanteBE));
            foreach (var nc in pendings)
            {
                try
                {
                    using (TextReader reader = new StringReader(nc.XmlContent))
                    {
                        ComprobanteBE r = (ComprobanteBE)serializer.Deserialize(reader);
                        if (r != null)
                        {
                            ComprobanteDto dto = new ComprobanteDto();
                            //var usersList = _usuariosRepository.GetUsuariosByUserId(nc.UserId);
                            var usersList = new DataResult<IEnumerable<UsersDto>>();
                            var users = usersList.Data;
                            dto.User = users.FirstOrDefault();
                            dto.esCopade = nc.EsCopade;
                            dto.esDocumental = nc.EsDocumental;
                            dto.comprobante = r;
                            dto.claveOrganismo = nc.ClaveOrganismo;
                            dto.notasCredito = null;
                            if (nc.NotasCredito != null)
                            {
                                if (nc.NotasCredito.Count > 0)
                                {
                                    List<ComprobanteBE> notasCredito = new List<ComprobanteBE>();
                                    foreach (var ncc in nc.NotasCredito)
                                    {
                                        using (TextReader readerNC = new StringReader(ncc))
                                        {
                                            ComprobanteBE rNC = (ComprobanteBE)serializer.Deserialize(readerNC);
                                            if (rNC != null)
                                            {
                                                notasCredito.Add(rNC);
                                            }
                                        }
                                    }
                                    if (notasCredito.Count > 0)
                                    {
                                        dto.notasCredito = notasCredito;
                                    }
                                }
                            }
                            comprobantes.Add(dto);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    errors.Add(nc.Id);
                }
            }

            _comprobantes = comprobantes;
            _errors = errors;
        }

        private async Task<DataResult<bool>> ReprocessInvoicePendings(List<PendingInvoiceDto> pendings)
        {
            bool completedProcess = true;

            DataResult<bool> result = new DataResult<bool>()
            {
                Status = HttpStatusCode.OK,
                Message = "Reproceso exitoso."
            };

            try
            {
                List<ComprobanteDto> comprobantes = new List<ComprobanteDto>();
                List<ComprobanteDto> results = new List<ComprobanteDto>();
                List<int> errors = new List<int>() { };

                ReprocessInvoiceLists(pendings, out comprobantes, out errors);

                if (comprobantes.Count > 0)
                {
                    foreach (var c in comprobantes)
                    {
                        var cr = await SaveInvoice(c);
                        results.Add(cr);
                    }
                }

                if (errors.Count > 0)
                {
                    await ReprocessinvoiceErrors(errors);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                completedProcess = false;
            }

            result.Data = completedProcess;
            return result;
        }

        private async Task<DataResult<bool>> ReprocessInvoicePendings200(List<PendingInvoiceDto> pendings)
        {
            DataResult<bool> result = new DataResult<bool>()
            {
                Status = HttpStatusCode.OK,
                Message = "Reproceso exitoso."
            };

            try
            {
                processBitacora = Convert.ToBoolean(_configuration["BitacoraInvoice:Habilitado"]);
            }
            catch (Exception exx)
            {
                Log.Error(exx.Message);
                processBitacora = true;
            }

            bool completedProcess = true;

            try
            {

                List<ComprobanteDto> comprobantes = new List<ComprobanteDto>();
                List<ComprobanteDto> results = new List<ComprobanteDto>();
                List<int> errors = new List<int>() { };
                IEnumerable<ValidationError> errorCatalog = new List<ValidationError>() { };

                ReprocessInvoiceLists(pendings, out comprobantes, out errors);

                var rslt = await GetValidationErrorCatalog();
                if (rslt.Status == HttpStatusCode.OK)
                {
                    errorCatalog = rslt.Data;
                }
                else
                {
                    result.Data = false;
                    return result;
                }

                if (comprobantes.Count > 0)
                {
                    foreach (var comprobante in comprobantes)
                    {
                        List<ValidationError> validationErrors = new List<ValidationError>();
                        string sapError = "";

                        CopadeDto copade = null;
                        string sourceDocument = "";
                        string reception = comprobante.comprobante.Addenda.Addenda_Pemex.ENTRADA.ToString();
                        string exercise = comprobante.comprobante.Addenda.Addenda_Pemex.EJERCICIO;
                        DataResult<CopadeDto> cpd = new DataResult<CopadeDto>();

                        PICopadeRequestDto pi = new PICopadeRequestDto() { Clave = comprobante.claveOrganismo, Reception = reception, Exercise = exercise };
                        cpd = await _sapPIRepository.RecuperaCopadeBDAsync(pi);

                        if (cpd.Status == HttpStatusCode.OK)
                        {
                            if (cpd.Data != null)
                            {
                                copade = cpd.Data;
                                sourceDocument = setSourceDocument(comprobante.comprobante, reception);
                                var invoices = await _invoiceRepository.GetInvoiceByReception(comprobante.claveOrganismo, reception, exercise);
                                var invoiceDto = invoices.Data;

                                //llamada a la api
                                await _bitacoraRepository.BitacoraInvoice(reception, "180", comprobante.User, processBitacora);
                                CXPDto cXP = setApiCXPDto(comprobante, copade, comprobante.comprobante.TipoDeComprobante, !comprobante.esDocumental, comprobante.esCopade, comprobante.User.Token);
                                var respApiCXP = await _sapPIRepository.PostCxP(cXP);
                                await _bitacoraRepository.BitacoraInvoice(reception, "190", comprobante.User, processBitacora);
                                string uuid = comprobante.comprobante.Complemento.TimbreFiscalDigital.UUID.ToString();
                                if (respApiCXP.Status != HttpStatusCode.OK || respApiCXP.Data == null)
                                {
                                    addError(false, errorCatalog, validationErrors, "60000", "60000", sourceDocument);
                                    validationErrors[validationErrors.Count - 1].descripcion = cxpError(uuid);
                                    validationErrors[validationErrors.Count - 1].mensaje = cxpError(uuid);
                                    sapError = "210";
                                    if (respApiCXP.Status != HttpStatusCode.OK)
                                    {
                                        addError(false, errorCatalog, validationErrors, "60000", "60000", sourceDocument);
                                        validationErrors[validationErrors.Count - 1].descripcion = cxpError(uuid);
                                        validationErrors[validationErrors.Count - 1].mensaje = cxpError(uuid);
                                    }
                                    if (respApiCXP.Data == null)
                                    {
                                        addError(false, errorCatalog, validationErrors, "60000", "60000", sourceDocument);
                                        validationErrors[validationErrors.Count - 1].descripcion = cxpError(uuid);
                                        validationErrors[validationErrors.Count - 1].mensaje = cxpError(uuid);
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(respApiCXP.Data.DocumentoSAP))
                                    {
                                        addError(false, errorCatalog, validationErrors, "60001", "60001", sourceDocument);
                                        validationErrors[validationErrors.Count - 1].descripcion = cxpError(uuid);
                                        validationErrors[validationErrors.Count - 1].mensaje = cxpError(uuid);
                                        sapError = "220";
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(respApiCXP.Data.Mensaje))
                                        {
                                            if (respApiCXP.Data.Mensaje.Trim().ToUpper() != "EXITO")
                                            {
                                                addError(false, errorCatalog, validationErrors, "60001", "60001", sourceDocument);
                                                validationErrors[validationErrors.Count - 1].descripcion = cxpError(uuid);
                                                validationErrors[validationErrors.Count - 1].mensaje = cxpError(uuid);
                                                sapError = "220";
                                            }
                                        }
                                        else
                                        {
                                            sapError = "200";
                                        }
                                    }
                                }
                                invoiceDto.SapDocument = respApiCXP.Data.DocumentoSAP;
                                if (!string.IsNullOrEmpty(invoiceDto.SapDocument))
                                {
                                    sapError = "200";
                                }

                                if (sapError != "")
                                {
                                    await _bitacoraRepository.BitacoraInvoice(reception, sapError, comprobante.User, processBitacora);
                                }
                                else
                                {
                                    InvoiceEstatusDto dto = new InvoiceEstatusDto();
                                    dto.InvoiceId = invoiceDto.InvoiceId;
                                    dto.Estatus = "0";
                                    await _facturaElectronicaRepository.SetInvoiceEstatus(dto);
                                    await _bitacoraRepository.BitacoraInvoice(reception, "330", comprobante.User, processBitacora);

                                    try
                                    {
                                        var validacionError = await _correoRepository.NotificacionFacturaEmailAsync(reception, validationErrors, comprobante.User, processBitacora, "", "");
                                    }
                                    catch (Exception ex)
                                    {
                                        var err = new ValidationError() { clave = "70000", descripcion = ex.Message, documento = "" };
                                        validationErrors.Add(err);
                                        Log.Error(ex.Message);
                                    }
                                }
                            }
                        }
                    }
                }

                if (errors.Count > 0)
                {
                    await ReprocessinvoiceErrors(errors);
                }

            }

            catch (Exception ex)
            {
                completedProcess = false;
                Log.Error(ex.Message);
            }

            result.Data = completedProcess;
            return result;

        }

        private async Task<DataResult<bool>> ReprocessInvoiceEmail()
        {
            bool completedProcess = true;

            DataResult<bool> resultItem = new DataResult<bool>()
            {
                Status = HttpStatusCode.OK
            };

            try
            {
                processBitacora = Convert.ToBoolean(_configuration["BitacoraInvoice:Habilitado"]);
            }
            catch (Exception exx)
            {
                processBitacora = true;
                Log.Error(exx.Message);
            }

            try
            {
                List<PendingEmailDto> pl = new List<PendingEmailDto>() { };
                using (IDbConnection db = GetConnection())
                {
                    var result = await db.QueryAsync<PendingEmailDto>(sql: "SP_invoice_get_pending_email", param: null, commandType: CommandType.StoredProcedure);

                    foreach (var r in result)
                    {
                        PendingEmailDto p = new PendingEmailDto();
                        p.DocumentoBEId = r.DocumentoBEId;
                        p.Estatus = r.Estatus;
                        p.Folio = r.Folio;
                        p.InvoiceId = r.InvoiceId;
                        p.LastStatus = r.LastStatus;
                        p.Reception = r.Reception;
                        p.Serie = r.Serie;
                        p.UserEmail = r.UserEmail;
                        p.UserId = r.UserId;
                        p.UUID = r.UUID;
                        pl.Add(p);
                    }

                    if (pl.Count > 0)
                    {
                        var rslt = await GetValidationErrorCatalog();
                        if (rslt.Status == HttpStatusCode.OK)
                        {
                            foreach (var p in pl)
                            {
                                IEnumerable<ValidationError> errorCatalog = rslt.Data;

                                string documento = p.Reception + " ";
                                if (p.UUID == Guid.Empty)
                                {
                                    documento = documento + " " + p.Serie + " " + p.Folio;
                                }
                                else
                                {
                                    documento = documento + " " + p.UUID;
                                }

                                List<ValidationError> validationErrors = await GetValidationErrorsByReception(errorCatalog, p.Reception, documento);
                                //var Users = await _usuariosRepository.GetUsuariosByUserId(p.UserId);
                                var Users = new DataResult<IEnumerable<UsersDto>>();
                                UsersDto user = Users.Data.FirstOrDefault();
                                try
                                {
                                    var validacionError = await _correoRepository.NotificacionFacturaEmailAsync(p.Reception, validationErrors, user, processBitacora, "", "");
                                }
                                catch (Exception ex)
                                {
                                    var err = new ValidationError() { clave = "70000", descripcion = ex.Message, documento = "" };
                                    validationErrors.Add(err);
                                    Log.Error(ex.Message);
                                }

                            }
                        }
                        else
                        {
                            resultItem.Data = false;
                            return resultItem;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                completedProcess = false;
                Log.Error(ex.Message);
            }

            resultItem.Data = completedProcess;
            return resultItem;
        }

        private async Task<List<ValidationError>> GetValidationErrorsByReception(IEnumerable<ValidationError> errorCatalog, string reception, string documento)
        {
            List<ValidationError> validationErrors = new List<ValidationError>() { };

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@reception", reception);
                    var result = await db.QueryAsync<BitacoraInvoiceErrorDto>(sql: "SP_invoice_get_pending_email_errors", param: par, commandType: CommandType.StoredProcedure);

                    foreach (var r in result)
                    {
                        ValidationError v = new ValidationError();
                        v.activo = true;
                        v.clave = r.clave;
                        var c = errorCatalog.Where(x => x.clave == r.clave).FirstOrDefault();
                        v.descripcion = string.IsNullOrEmpty(r.mensaje) ? c.descripcion : r.mensaje;
                        v.documento = documento;
                        v.esTerminal = true;
                        v.id = r.id;
                        v.mensaje = r.mensaje;

                        validationErrors.Add(v);
                    }
                }
            }
            catch (Exception ex)
            {
                validationErrors.Clear();
                Log.Error(ex.Message);
            }

            return validationErrors;
        }

        private async Task<bool> ReprocessinvoiceErrors(List<int> errors)
        {
            bool completedProcess = true;

            try
            {
                List<PendingEmailDto> pl = new List<PendingEmailDto>() { };
                using (IDbConnection db = GetConnection())
                {
                    foreach (var item in errors)
                    {
                        DynamicParameters par = new DynamicParameters();
                        par.Add("@Id", item);
                        await db.QueryAsync<PendingEmailDto>(sql: "SP_invoice_set_pending_error", param: par, commandType: CommandType.StoredProcedure);
                    }
                }

            }
            catch (Exception ex)
            {
                completedProcess = false;
                Log.Error(ex.Message);
            }
            return completedProcess;
        }

        public async Task<bool> setInitialRequestRejected(string reception, string correo, string RFCEmisor, string RFCReceptor)
        {
            bool completedProcess = true;

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@Reception", reception);
                    par.Add("@Correo", correo);
                    par.Add("@RFCEmisor", RFCEmisor);
                    par.Add("@RFCReceptor", RFCReceptor);
                    await db.QueryAsync<PendingEmailDto>(sql: "SP_invoice_set_initialrequest_rejected", param: par, commandType: CommandType.StoredProcedure);
                }

            }
            catch (Exception ex)
            {
                completedProcess = false;
                Log.Error(ex.Message);
            }
            return completedProcess;
        }

        public async Task<DataResult<InvoiceValidateDto>> ValidateInvoiceAP(DataResult<InvoiceValidateDto> dto, string idAnalitico, string sourceDocument, ComprobanteBE comprobante, string comprobanteOriginal)
        {
            IEnumerable<ValidationError> errorCatalog = new List<ValidationError>() { };
            List<ValidationError> validationErrors = new List<ValidationError>();
            bool existsCriticalError = false;
            bool esNotaCredito = false;
            AnaliticoPagoDto analitico = null;

            if (!string.IsNullOrEmpty(dto.Data.invoiceDto.TipoComprobante))
            {
                if (dto.Data.invoiceDto.TipoComprobante.ToUpper() == "E")
                    esNotaCredito = true;
            }

            DataResult<InvoiceValidateDto> resultItem = new DataResult<InvoiceValidateDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Validacion exitosa."
            };
            try
            {
                var rslt = await GetValidationErrorCatalog();
                if (rslt.Status != HttpStatusCode.OK)
                {
                    validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "90000", "90000", sourceDocument);
                    existsCriticalError = true;
                }
                else
                {
                    errorCatalog = rslt.Data;
                    if (errorCatalog == null)
                    {
                        validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "90001", "90001", sourceDocument);
                        existsCriticalError = true;
                    }
                }

                if (!existsCriticalError)
                {
                    await _bitacoraRepository.BitacoraInvoice(dto.Data.recepcion, "40", dto.User, processBitacora);

                    DataResult<AnaliticoPagoDto> apd = new DataResult<AnaliticoPagoDto>();
                    apd = await _analiticoPagoRepository.GetAPByIdAnaliticoAsync(idAnalitico);

                    if (apd.Status != HttpStatusCode.OK)
                    {
                        validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "12200", "12200", sourceDocument);
                        existsCriticalError = true;
                    }
                    else
                    {
                        if (apd.Data == null)
                        {
                            validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "12200", "12200", sourceDocument);
                            existsCriticalError = true;
                        }
                        else
                        {
                            analitico = apd.Data;
                            if (analitico == null)
                            {
                                validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "12200", "12200", sourceDocument);
                                existsCriticalError = true;
                            }
                            else
                            {
                                if (analitico.IsCancel == true)
                                {
                                    validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "12201", "12201", sourceDocument);
                                    existsCriticalError = true;
                                }
                                else
                                {
                                    if (analitico.FunctionarySignDate == null)
                                    {
                                        validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "12208", "12208", sourceDocument);
                                        existsCriticalError = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!existsCriticalError)
                {
                    await _bitacoraRepository.BitacoraInvoice(dto.Data.recepcion, "140", dto.User, processBitacora);

                    if (string.IsNullOrEmpty(dto.Data.invoiceDto.Subtotal)) dto.Data.invoiceDto.Subtotal = "0";
                    if (string.IsNullOrEmpty(dto.Data.invoiceDto.Total)) dto.Data.invoiceDto.Total = "0";
                    if (string.IsNullOrEmpty(dto.Data.invoiceDto.TotalImpuestosTrasladados)) dto.Data.invoiceDto.TotalImpuestosTrasladados = "0";
                    if (string.IsNullOrEmpty(dto.Data.invoiceDto.TotalImpuestosRetenidos)) dto.Data.invoiceDto.TotalImpuestosRetenidos = "0";
                    if (string.IsNullOrEmpty(dto.Data.invoiceDto.TipoComprobante)) dto.Data.invoiceDto.TipoComprobante = "";

                    int qtyErrors = validationErrors.Count;
                    validationErrors = await validateDetailComprobateAP(errorCatalog, validationErrors, dto.Data, analitico, comprobante, sourceDocument, esNotaCredito, comprobanteOriginal);
                    if (qtyErrors != validationErrors.Count)
                    {
                        var er = validationErrors.Where(x => x.esTerminal).Count();
                        if(er>0) existsCriticalError = true;
                    }

                    await _bitacoraRepository.BitacoraInvoice(dto.Data.recepcion, "150", dto.User, processBitacora);
                }

                dto.Data.validationErrors = validationErrors;
                resultItem.Data = dto.Data;

                return resultItem;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }

        public string cxpError(string uuid)
        {
            if (uuid == Guid.Empty.ToString())
            {
                return "Su factura se encuentra en proceso de revisión. Una vez que este finalice exitosamente le será notificada su aceptación";
            }
            else
            {
                return "Su factura con UUID " + uuid.ToUpper() + " se encuentra en proceso de revisión. Una vez que este finalice exitosamente le será notificada su aceptación";
            }
        }

        public CXPResultDto setCxPResult(HttpStatusCode Status, CXPDto Data, string uuid, IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, string documento, bool existsCriticalError)
        {
            CXPResultDto cXPResultDto = new CXPResultDto() { existsCriticalError = existsCriticalError, sapDocument = "", sapError = "", validationErrors = validationErrors };

            if (Status != HttpStatusCode.OK || Data == null)
            {
                validationErrors = addError(false, errorCatalog, validationErrors, "60000", "60000", documento);
                validationErrors[validationErrors.Count - 1].descripcion = cxpError(uuid);
                validationErrors[validationErrors.Count - 1].mensaje = cxpError(uuid);
                cXPResultDto.sapError = "210";
                if (Status != HttpStatusCode.OK)
                {
                    validationErrors = addError(false, errorCatalog, validationErrors, "60000", "60000", documento);
                    validationErrors[validationErrors.Count - 1].descripcion = cxpError(uuid);
                    validationErrors[validationErrors.Count - 1].mensaje = cxpError(uuid);
                }
                if (Data == null)
                {
                    validationErrors = addError(false, errorCatalog, validationErrors, "60000", "60000", documento);
                    validationErrors[validationErrors.Count - 1].descripcion = cxpError(uuid);
                    validationErrors[validationErrors.Count - 1].mensaje = cxpError(uuid);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Data.DocumentoSAP))
                {
                    validationErrors = addError(false, errorCatalog, validationErrors, "60001", "60001", documento);
                    validationErrors[validationErrors.Count - 1].descripcion = cxpError(uuid);
                    validationErrors[validationErrors.Count - 1].mensaje = cxpError(uuid);
                    cXPResultDto.sapError = "220";
                }
                else
                {
                    if (!string.IsNullOrEmpty(Data.Mensaje))
                    {
                        if (Data.Mensaje.Trim().ToUpper() != "EXITO")
                        {
                            validationErrors = addError(false, errorCatalog, validationErrors, "60001", "60001", documento);
                            validationErrors[validationErrors.Count - 1].descripcion = cxpError(uuid);
                            validationErrors[validationErrors.Count - 1].mensaje = cxpError(uuid);
                            cXPResultDto.sapError = "220";
                        }
                    }
                    else
                    {
                        cXPResultDto.sapError = "200";
                    }
                }
                if (!string.IsNullOrEmpty(Data.DocumentoSAP))
                {
                    cXPResultDto.sapError = "200";
                }
            }

            cXPResultDto.validationErrors = validationErrors;

            return cXPResultDto;
        }

        public async Task<SATResultDto> setSATResult(string RFCEmisor, string RFCReceptor, string Total, Guid UUID, List<ValidationError> validationErrors, IEnumerable<ValidationError> errorCatalog, bool existsCriticalError, string sourceDocument)
        {
            ValidacionSATDto vsd = new ValidacionSATDto();

            SATResultDto result = new SATResultDto();
            result.existsCriticalError = existsCriticalError;

            try
            {
                var rSAT = await _sATRepository.GetValidacionSATAsync(vsd);
                if (rSAT.Status == HttpStatusCode.OK)
                {
                    if (rSAT.Data.CodigoEstatus.Substring(0, 1).ToUpper() != "S")
                    {
                        validationErrors.Add(errorCatalog.Where(x => x.clave == rSAT.Data.CodigoEstatus).FirstOrDefault());
                        validationErrors[validationErrors.Count - 1].documento = sourceDocument;
                        existsCriticalError = true;
                    }
                }
                else
                {
                    validationErrors = addError(false, errorCatalog, validationErrors, "20600", "20600", sourceDocument);
                    existsCriticalError = true;
                }
            }
            catch (Exception ex)
            {
                validationErrors = addError(false, errorCatalog, validationErrors, "20600", "20600", sourceDocument);
                existsCriticalError = true;
                Log.Error(ex.Message);
            }

            result.existsCriticalError = existsCriticalError;
            result.validationErrors = validationErrors;

            return result;
        }


    }
}
