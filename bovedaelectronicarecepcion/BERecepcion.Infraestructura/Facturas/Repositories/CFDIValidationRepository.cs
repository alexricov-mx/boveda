using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class CFDIValidationRepository : BaseSQLServerSqlRepository, ICFDIValidationRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ICFDIValidationCartaPorteRepository _cartaPorteRepository;
        private readonly ICFDIValidation40Repository _validation40Repository;

        public CFDIValidationRepository(string cnnString, IConfiguration configuration, 
            ICFDIValidationCartaPorteRepository cartaPorteRepository,
            ICFDIValidation40Repository validation40Repository) : base(cnnString)
        {
            _configuration = configuration;
            _cartaPorteRepository = cartaPorteRepository;
            _validation40Repository = validation40Repository;
        }

        public async Task<List<ValidationError>> validateDetailRFC(string receptorRFC, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, string documento, bool esNotaCredito)
        {
            string rfcPattern = @"^([A-ZÑ\x26]{3,4}([0-9]{2})(0[1-9]|1[0-2])(0[1-9]|1[0-9]|2[0-9]|3[0-1]))([A-Z\d]{3})?$";

            //14.Valida que el RFC del Emisor corresponda con el acreedor y que el RFC del Receptor corresponda con la EPS de origen.

            if (copade.Rfc == null) copade.Rfc = "";
            if (copade.CreditorRfc == null) copade.CreditorRfc = "";

            if (string.IsNullOrEmpty(comprobanteBE.Emisor.Rfc))
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10000", "20000", documento);
            Match me = Regex.Match(comprobanteBE.Emisor.Rfc, rfcPattern, RegexOptions.IgnoreCase);
            if (!me.Success)
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10001", "20001", documento);
            if (string.IsNullOrEmpty(comprobanteBE.Emisor.Nombre))
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10002", "20002", documento);
            if (comprobanteBE.Emisor.Rfc.ToUpper() != copade.CreditorRfc.ToUpper())
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10003", "20003", documento);

            if (string.IsNullOrEmpty(receptorRFC))
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10100", "20100", documento);
            Match mr = Regex.Match(receptorRFC, rfcPattern, RegexOptions.IgnoreCase);
            if (!mr.Success)
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10101", "20101", documento);
            if (string.IsNullOrEmpty(comprobanteBE.Receptor.Nombre))
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10102", "20102", documento);
            if (comprobanteBE.Receptor.Rfc.ToUpper() != receptorRFC.ToUpper())
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10103", "20103", documento);

            return validationErrors;
        }

        public async Task<List<ValidationError>> validateDetailHeader(string clave, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, string documento, bool esNotaCredito)
        {
            //19.Valida Moneda
            if (string.IsNullOrEmpty(comprobanteBE.Moneda))
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10306", "20306", documento);
            if (comprobanteBE.Moneda.ToUpper().Trim() != copade.Currency.ToUpper().Trim())
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10307", "20307", documento);

            //20.Valida Subtotal
            if (string.IsNullOrEmpty(copade.Subtotal))
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10301", "20301", documento);
            }
            else
            {
                if (Math.Abs(comprobanteBE.SubTotal - Convert.ToDouble(copade.Subtotal)) >= Convert.ToDouble(1))
                    validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10302", "20302", documento);
            }
            //21.Valida total
            if (string.IsNullOrEmpty(copade.Total))
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10400", "20400", documento);
            }
            else
            {
                if (Math.Abs(comprobanteBE.Total - Convert.ToDouble(copade.Total)) >= Convert.ToDouble(1))
                    validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10402", "20402", documento);
            }

            //22.Valida Impuestos
            if (!string.IsNullOrEmpty(copade.TaxTotalTransferred) || comprobanteBE.Impuestos.TotalImpuestosTrasladados != 0)
            {
                if (string.IsNullOrEmpty(copade.TaxTotalTransferred))
                {
                    validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10410", "20410", documento);
                }
                else
                {
                    if (Math.Abs(comprobanteBE.Impuestos.TotalImpuestosTrasladados - Convert.ToDouble(copade.TaxTotalTransferred)) >= Convert.ToDouble(1))
                        validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10411", "20411", documento);
                }
            }

            if (!string.IsNullOrEmpty(copade.TaxTotalRetained) || comprobanteBE.Impuestos.TotalImpuestosRetenidos != 0)
            {
                if (string.IsNullOrEmpty(copade.TaxTotalRetained))
                {
                    validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10420", "20420", documento);
                }
                else
                {
                    if (comprobanteBE.Impuestos.TotalImpuestosRetenidos - Convert.ToDouble(copade.TaxTotalLocalRetained) >= Convert.ToDouble(1))
                        validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10421", "20421", documento);
                }
            }

            return validationErrors;
        }

        public async Task<List<ValidationError>> validateDetailConcepts(string clave, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, string documento, bool esNotaCredito)
        {
            //15.Valida el número de conceptos
            //debe coincidir la cantidad de conceptos del copade prefactura contra la factura
            //barrer la lista de prefactura y buscar en la factura que exista el concepto que coincida con los atributos -debe coincidir todos
            //la diferencia de montos debe ser menor o igual a 0.99
            if (comprobanteBE.Conceptos.Concepto.Count == 0)
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10200", "20200", documento);

            bool conceptoNulo = false;
            int qtyCopadeConceptoCoincidencia = 0;
            List<int> renglonRevisado = new List<int>() { };
            foreach (var itemCopade in copade.vPreFactura.comprobante.conceptos)
            {
                int renglon = 0;
                foreach (var item in comprobanteBE.Conceptos.Concepto)
                {
                    double monto = Convert.ToDouble(item.Importe);
                    if (esNotaCredito)
                    {
                        monto = comprobanteBE.SubTotal;
                    }
                    if (item.Descripcion == null)
                    {
                        conceptoNulo = true;
                    }
                    //cantidad, valor, importe y unidad
                    if (Math.Abs(Convert.ToDouble(item.Cantidad) - Convert.ToDouble(itemCopade.cantidad)) < Convert.ToDouble(1)
                        && Math.Abs(Convert.ToDouble(item.ValorUnitario) - Convert.ToDouble(itemCopade.valorUnitario)) < Convert.ToDouble(1)
                        && Math.Abs(monto - Convert.ToDouble(itemCopade.importe)) < Convert.ToDouble(1))
                    {
                        bool found = false;
                        if (renglonRevisado.Count > 0)
                        {
                            for (int i = 0; i < renglonRevisado.Count; i++)
                            {
                                if (renglonRevisado[i] == renglon) found = true;
                            }
                        }

                        if (!found)
                        {
                            //16.Valida Cantidad
                            if (Math.Abs(Convert.ToDouble(item.Cantidad) - Convert.ToDouble(itemCopade.cantidad)) >= Convert.ToDouble(1))
                                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10205", "20205", documento);

                            //17.Valida valor unitario
                            if (Math.Abs(item.ValorUnitario - Convert.ToDouble(itemCopade.valorUnitario)) >= Convert.ToDouble(1))
                                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10206", "20206", documento);

                            //18.Valida Importe
                            if (Math.Abs(monto - Convert.ToDouble(itemCopade.importe)) >= Convert.ToDouble(1))
                                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10204", "20204", documento);

                            renglonRevisado.Add(renglon);
                            qtyCopadeConceptoCoincidencia++;
                            break;
                        }
                    }
                    renglon++;
                }
            }

            if (comprobanteBE.Conceptos.Concepto.Count != qtyCopadeConceptoCoincidencia ||
                    copade.vPreFactura.comprobante.conceptos.ToList().Count != comprobanteBE.Conceptos.Concepto.ToList().Count)
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10303", "20303", documento);
            }

            if (conceptoNulo == true)
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "12304", "12304", documento);
            }

            return validationErrors;
        }

        public async Task<List<ValidationError>> validateDetailComprobante(string clave, string receptorRFC, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobante, CopadeDto copade, string documento, bool esNotaCredito, string ComprobanteOriginal)
        {
            validationErrors = await validateDetailRFC(receptorRFC, catalogErrors, validationErrors, comprobante, copade, documento, esNotaCredito);
            validationErrors = await validateDetailHeader(clave, catalogErrors, validationErrors, comprobante, copade, documento, esNotaCredito);
            validationErrors = await validateDetailConcepts(clave, catalogErrors, validationErrors, comprobante, copade, documento, esNotaCredito);
            if (comprobante.Complemento != null)
                if (comprobante.Complemento.CartaPorte != null)   
                    validationErrors = await _cartaPorteRepository.validateCartaPorte(catalogErrors, validationErrors, comprobante.Complemento.CartaPorte, documento, esNotaCredito);

            try
            {
                if (String.IsNullOrEmpty(ComprobanteOriginal) && Convert.ToDecimal(comprobante.Version) >= 4)
                {
                    Log.Error("ComprobanteOriginal " + comprobante.Version + " no proporcionado");
                }
                else
                {
                    if (comprobante.Version == "4.0")
                    {
                        ComprobanteBE40 comprobanteBE40 = JsonConvert.DeserializeObject<ComprobanteBE40>(ComprobanteOriginal);
                        validationErrors = await _validation40Repository.validateComprobante(catalogErrors, validationErrors, comprobanteBE40, documento, esNotaCredito);
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
            }
            
            return validationErrors;
        }

        public async Task<List<ValidationError>> validateDetailNCComprobante(string clave, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, ComprobanteBE comprobanteBE, CopadeDto copade, bool esNotaCredito, string cfdiEmisorRFC, string cfdiReceptorRFC, string cfdiMoneda, string documento)
        {
            //validacion de los rfc que sean los mismos que el documento padre
            if (comprobanteBE.Emisor.Rfc.ToUpper() != cfdiEmisorRFC.ToUpper())
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10003", "20003", documento);
            if (comprobanteBE.Receptor.Rfc.ToUpper() != cfdiReceptorRFC.ToUpper())
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10103", "20103", documento);
            //19.Valida Moneda
            if (string.IsNullOrEmpty(comprobanteBE.Moneda))
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10306", "20306", documento);
            if (comprobanteBE.Moneda.ToUpper().Trim() != cfdiMoneda.ToUpper().Trim())
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10307", "20307", documento);

            //15.Valida el número de conceptos
            //debe coincidir la cantidad de conceptos del copade prefactura contra la factura
            //barrer la lista de prefactura y buscar en la factura que exista el concepto que coincida con los atributos -debe coincidir todos
            //la diferencia de montos debe ser menor o igual a 0.99
            if (comprobanteBE.Conceptos.Concepto.Count == 0)
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10200", "20200", documento);

            int qtyCopadeConceptoCoincidencia = 0;
            List<int> renglonRevisado = new List<int>() { };
            foreach (var itemCopade in copade.vPreFactura.comprobante.conceptos)
            {
                int renglon = 0;
                foreach (var item in comprobanteBE.Conceptos.Concepto)
                {
                    //cantidad, valor, importe y unidad
                    if (Math.Abs(Convert.ToDouble(item.Cantidad) - Convert.ToDouble(itemCopade.cantidad)) < Convert.ToDouble(1)
                        && Math.Abs(Convert.ToDouble(item.ValorUnitario) - Convert.ToDouble(itemCopade.valorUnitario)) < Convert.ToDouble(1)
                        && Math.Abs(Convert.ToDouble(item.Importe) - Convert.ToDouble(itemCopade.importe)) < Convert.ToDouble(1))
                    {
                        bool found = false;
                        if (renglonRevisado.Count > 0)
                        {
                            for (int i = 0; i < renglonRevisado.Count; i++)
                            {
                                if (renglonRevisado[i] == renglon) found = true;
                            }
                        }

                        if (!found)
                        {
                            //16.Valida Cantidad
                            if (Math.Abs(Convert.ToDouble(item.Cantidad) - Convert.ToDouble(itemCopade.cantidad)) >= Convert.ToDouble(1))
                                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10205", "20205", documento);

                            //17.Valida valor unitario
                            if (Math.Abs(item.ValorUnitario - Convert.ToDouble(itemCopade.valorUnitario)) >= Convert.ToDouble(1))
                                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10206", "20206", documento);

                            //18.Valida Importe
                            if (Math.Abs(item.Importe - Convert.ToDouble(itemCopade.importe)) >= Convert.ToDouble(1))
                                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10204", "20204", documento);

                            renglonRevisado.Add(renglon);
                            qtyCopadeConceptoCoincidencia++;
                            break;
                        }
                    }
                    renglon++;
                }
            }

            if (comprobanteBE.Conceptos.Concepto.Count != qtyCopadeConceptoCoincidencia ||
                    copade.vPreFactura.comprobante.conceptos.ToList().Count != comprobanteBE.Conceptos.Concepto.ToList().Count)
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10303", "20303", documento);
            }

            //21.Valida total
            if (string.IsNullOrEmpty(copade.Total))
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10400", "20400", documento);
            }
            else
            {
                if (Math.Abs(comprobanteBE.Total - Convert.ToDouble(copade.Total)) >= Convert.ToDouble(1))
                    validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10402", "20402", documento);
            }

            return validationErrors;
        }

        public async Task<List<ValidationError>> validateDetailHeaderAP(IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, InvoiceValidateDto dto, AnaliticoPagoDto analitico, ComprobanteBE comprobante, string sourceDocument, bool esNotaCredito)
        {
            //19.Valida Moneda
            if (string.IsNullOrEmpty(comprobante.Moneda))
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10306", "20306", sourceDocument);
            if (comprobante.Moneda.ToUpper().Trim() != analitico.Moneda.ToUpper().Trim())
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "10307", "20307", sourceDocument);

            //20.Valida Subtotal
            if (string.IsNullOrEmpty(analitico.SubTotal))
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "12301", "12301", sourceDocument);
            }
            else
            {
                if (Math.Abs(Convert.ToDouble(dto.invoiceDto.Subtotal) - Convert.ToDouble(analitico.SubTotal)) >= Convert.ToDouble(1))
                    validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "12302", "12302", sourceDocument);
            }
            //21.Valida total
            if (string.IsNullOrEmpty(analitico.Total))
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "12400", "12400", sourceDocument);
            }
            else
            {
                if (Math.Abs(Convert.ToDouble(dto.invoiceDto.Total) - Convert.ToDouble(analitico.Total)) >= Convert.ToDouble(1))
                    validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "12402", "12402", sourceDocument);
            }

            //22.Valida Impuestos
            if (!string.IsNullOrEmpty(analitico.vImpuestos.totalImpuestosTrasladados) || Convert.ToDouble(dto.invoiceDto.TotalImpuestosTrasladados) != 0)
            {
                if (string.IsNullOrEmpty(analitico.vImpuestos.totalImpuestosTrasladados))
                {
                    validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "12410", "12410", sourceDocument);
                }
                else
                {
                    if (Math.Abs(Convert.ToDouble(dto.invoiceDto.TotalImpuestosTrasladados) - Convert.ToDouble(analitico.vImpuestos.totalImpuestosTrasladados)) >= Convert.ToDouble(1))
                        validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "12411", "12411", sourceDocument);
                }
            }

            if (!string.IsNullOrEmpty(analitico.vImpuestos.totalImpuestosRetenidos) || Convert.ToDouble(dto.invoiceDto.TotalImpuestosRetenidos) != 0)
            {
                if (string.IsNullOrEmpty(analitico.vImpuestos.totalImpuestosRetenidos))
                {
                    validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "12420", "12420", sourceDocument);
                }
                else
                {
                    if (Convert.ToDouble(dto.invoiceDto.TotalImpuestosRetenidos) - Convert.ToDouble(analitico.vImpuestos.totalImpuestosRetenidos) >= Convert.ToDouble(1))
                        validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "12421", "12421", sourceDocument);
                }
            }

            //adicionales

            if (!(dto.invoiceDto.TipoComprobante.ToUpper() == "I" || dto.invoiceDto.TipoComprobante.ToUpper() == "E"))
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "50000", "50000", sourceDocument);
            }
            if (dto.invoiceDto.TipoComprobante.ToUpper() == "I" && esNotaCredito)
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "50001", "50001", sourceDocument);
            }
            if (dto.invoiceDto.UserId == Guid.Empty)
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "50002", "50002", sourceDocument);
            }
            if (string.IsNullOrEmpty(dto.invoiceDto.OriginalXML))
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "50003", "50003", sourceDocument);
            }
            if (dto.invoiceDto.Uuid == Guid.Empty)
            {
                validationErrors = addError(esNotaCredito, catalogErrors, validationErrors, "50004", "50004", sourceDocument);
            }

            return validationErrors;
        }

        public async Task<List<ValidationError>> validateDetailConceptsAP(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, InvoiceValidateDto dto, AnaliticoPagoDto analitico, string sourceDocument, bool esNotaCredito)
        {
            //15.Valida el número de conceptos
            //debe coincidir la cantidad de conceptos del copade prefactura contra la factura
            //barrer la lista de prefactura y buscar en la factura que exista el concepto que coincida con los atributos -debe coincidir todos
            //la diferencia de montos debe ser menor o igual a 0.99
            if (dto.conceptos.Concepto.Count == 0)
                validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "10200", "10200", sourceDocument);

            int qtyCopadeConceptoCoincidencia = 0;
            List<int> renglonRevisado = new List<int>() { };
            bool conceptoNulo = false;
            foreach (var itemAnalitico in analitico.vPreFactura.comprobante.conceptos)
            {
                int renglon = 0;
                foreach (var item in dto.conceptos.Concepto)
                {
                    if (item.Descripcion == null)
                    {
                        conceptoNulo = true;
                    }
                    //cantidad, valor, importe y unidad
                    if (Math.Abs(Convert.ToDouble(item.Cantidad) - Convert.ToDouble(itemAnalitico.cantidad)) < Convert.ToDouble(1)
                        && Math.Abs(Convert.ToDouble(item.ValorUnitario) - Convert.ToDouble(itemAnalitico.valorUnitario)) < Convert.ToDouble(1)
                        && Math.Abs(Convert.ToDouble(item.Importe) - Convert.ToDouble(itemAnalitico.importe)) < Convert.ToDouble(1))
                    {
                        bool found = false;
                        if (renglonRevisado.Count > 0)
                        {
                            for (int i = 0; i < renglonRevisado.Count; i++)
                            {
                                if (renglonRevisado[i] == renglon) found = true;
                            }
                        }

                        if (!found)
                        {
                            //16.Valida Cantidad
                            if (Math.Abs(Convert.ToDouble(item.Cantidad) - Convert.ToDouble(itemAnalitico.cantidad)) >= Convert.ToDouble(1))
                                validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "12205", "12205", sourceDocument);

                            //17.Valida valor unitario
                            if (Math.Abs(item.ValorUnitario - Convert.ToDouble(itemAnalitico.valorUnitario)) >= Convert.ToDouble(1))
                                validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "12206", "12206", sourceDocument);

                            //18.Valida Importe
                            if (Math.Abs(item.Importe - Convert.ToDouble(itemAnalitico.importe)) >= Convert.ToDouble(1))
                                validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "12204", "12204", sourceDocument);

                            renglonRevisado.Add(renglon);
                            qtyCopadeConceptoCoincidencia++;
                            break;
                        }
                    }
                    renglon++;
                }
            }

            if (dto.conceptos.Concepto.Count != qtyCopadeConceptoCoincidencia ||
                    analitico.vPreFactura.comprobante.conceptos.ToList().Count != dto.conceptos.Concepto.ToList().Count)
            {
                validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "12303", "12303", sourceDocument);
            }

            if (conceptoNulo == true)
            {
                validationErrors = addError(esNotaCredito, errorCatalog, validationErrors, "12303", "12303", sourceDocument);
            }

            return validationErrors;
        }

        public async Task<List<ValidationError>> validateDetailComprobateAP(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, InvoiceValidateDto dto, AnaliticoPagoDto analitico, ComprobanteBE comprobante, string sourceDocument, bool esNotaCredito, string ComprobanteOriginal)
        {
            validationErrors = await validateDetailHeaderAP(errorCatalog, validationErrors, dto, analitico, comprobante, sourceDocument, esNotaCredito);
            validationErrors = await validateDetailConceptsAP(errorCatalog, validationErrors, dto, analitico, sourceDocument, esNotaCredito);
            if (comprobante.Complemento != null)
                if (comprobante.Complemento.CartaPorte != null)
                    validationErrors = await _cartaPorteRepository.validateCartaPorte(errorCatalog, validationErrors, comprobante.Complemento.CartaPorte, sourceDocument, false);

            try
            {
                if (String.IsNullOrEmpty(ComprobanteOriginal) && Convert.ToDecimal(comprobante.Version) >= 4)
                {
                    Log.Error("ComprobanteOriginal " + comprobante.Version + " no proporcionado");
                }
                else
                {
                    if (comprobante.Version == "4.0")
                    {
                        ComprobanteBE40 comprobanteBE40 = JsonConvert.DeserializeObject<ComprobanteBE40>(ComprobanteOriginal);
                        validationErrors = await _validation40Repository.validateComprobante(errorCatalog, validationErrors, comprobanteBE40, sourceDocument, esNotaCredito);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            
            return validationErrors;
        }

        public List<ValidationError> addError(bool esNotaCredito, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, string clave, string claveNC, string documento)
        {
            var item = catalogErrors.Where(x => x.clave == (!esNotaCredito ? clave : claveNC)).FirstOrDefault();

            if (item != null)
            {
                var previous = validationErrors.Where(x => x.clave == (!esNotaCredito ? clave : claveNC)).FirstOrDefault();
                if (previous == null)
                {
                    validationErrors.Add(item);
                    validationErrors[validationErrors.Count - 1].documento = documento;
                }
            }

            return validationErrors;
        }
    }
}
