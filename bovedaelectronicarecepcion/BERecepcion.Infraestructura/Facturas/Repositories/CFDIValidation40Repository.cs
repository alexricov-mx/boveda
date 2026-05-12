using System.Text;
using System.Threading.Tasks;
using Serilog;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data;
using Dapper;
using BERecepcion.Core.Facturas.Interfaces.Repositories;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Infraestructura.Repositories;
using BERecepcion.Core.Models;
using System.Collections.Generic;
using System;
using BERecepcion.Infraestructura.Utils;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class CFDIValidation40Repository : BaseSQLServerSqlRepository, ICFDIValidation40Repository
    {
        public CFDIValidation40Repository(string cnnString) : base(cnnString)
        {
        }

        string pattern = "";
        List<DataSearch> dataSearch = null;
        List<DataSearchRegExp> dataPattern = null;

        private List<ValidationError> validateComprobanteHdr(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, ComprobanteBE40 comprobante, string sourceDocument, bool esNotaCredito)
        {
            if (String.IsNullOrEmpty(comprobante.Version))
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Version requerida");
            }

            if (!String.IsNullOrEmpty(comprobante.Serie))
            {
                if (comprobante.Serie.Length > 25)
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud de la serie mayor a 25 caracteres");
                }
            }

            if (!String.IsNullOrEmpty(comprobante.Folio))
            {
                if (comprobante.Folio.Length > 40)
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud del folio mayor a 40 caracteres");
                }
            }

            if (comprobante.Fecha == DateTime.MinValue)
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Fecha no proporcionada");
            }

            if (String.IsNullOrEmpty(comprobante.Sello))
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Sello requerido");
            }

            if (comprobante.FormaPagoSpecified)
            {
                if (String.IsNullOrEmpty(comprobante.FormaPago))
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Forma de pago no especificada");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_FormaPago.ToString(), "FormaPago", comprobante.FormaPago);
                }
            }

            if (String.IsNullOrEmpty(comprobante.NoCertificado))
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero de certificado no especificado");
            }
            else
            {
                if (comprobante.NoCertificado.Length != 20)
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud del numero de certificado diferente a 20 caracteres");
                }
            }

            if (!String.IsNullOrEmpty(comprobante.CondicionesDePago))
            {
                if (comprobante.CondicionesDePago.Length > 1000)
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud de las condiciones de pago mayor a 1000 caracteres");
                }
            }

            if (comprobante.SubTotal < 0)
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Valor del subtotal negativo no permitido");
            }

            if (comprobante.Descuento < 0)
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Valor del descuento negativo no permitido");
            }

            if (String.IsNullOrEmpty(comprobante.Moneda))
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Moneda requerida");
            }
            else
            {
                dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Moneda.ToString(), "Moneda", comprobante.Moneda);
                if (!(comprobante.Moneda == "MXN" || comprobante.Moneda == "XXX"))
                {
                    if (!comprobante.TipoCambioSpecified)
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo de cambio requerido");
                    }
                    else
                    {
                        if (comprobante.TipoCambio == 0)
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo de cambio en ceros no permitido");
                        }
                    }
                }
            }

            if (comprobante.Total < 0)
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Total negativo no permitido");
            }

            if (String.IsNullOrEmpty(comprobante.TipoDeComprobante))
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo de comprobante requerido");
            }
            else
            {
                dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoDeComprobante.ToString(), "TipoDeComprobante", comprobante.TipoDeComprobante);
            }

            if (String.IsNullOrEmpty(comprobante.Exportacion))
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Indicador de exportacion requerido");
            }
            else
            {
                dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Exportacion.ToString(), "Exportacion", comprobante.Exportacion);
            }

            if (comprobante.MetodoPagoSpecified)
            {
                if (String.IsNullOrEmpty(comprobante.MetodoPago))
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Indicador de exportacion requerido");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_MetodoPago.ToString(), "MetodoPago", comprobante.MetodoPago);
                }
            }

            if (String.IsNullOrEmpty(comprobante.LugarExpedicion))
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "LugarExpedicion requerido");
            }
            else
            {
                dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_CodigoPostal.ToString(), "LugarExpedicion", comprobante.LugarExpedicion);
            }

            if (!String.IsNullOrEmpty(comprobante.Confirmacion))
            {
                pattern = getPattern(dataPattern, "Comprobante40.Confirmacion");
                if (!string.IsNullOrEmpty(pattern))
                {
                    Match me = Regex.Match(comprobante.Confirmacion, pattern, RegexOptions.IgnoreCase);
                    if (!me.Success)
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Formato de confirmacion en el comprobante incorrecto");
                    }
                }
                else
                {
                    Log.Error("Error de carga de patron para Comprobante40.Confirmacion");
                }
            }

            return validationErrors;
        }
        private List<ValidationError> validateComprobanteInformacionGlobal(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, BERecepcion.Core.Facturas.Dto.ComprobanteInformacionGlobal InformacionGlobal, string sourceDocument, bool esNotaCredito)
        {
            if (InformacionGlobal != null)
            {
                if (String.IsNullOrEmpty(InformacionGlobal.Periodicidad))
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Periodicidad en InformacionGlobal requerido");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Periodicidad.ToString(), "Periodicidad", InformacionGlobal.Periodicidad);
                }

                if (String.IsNullOrEmpty(InformacionGlobal.Meses))
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Meses en InformacionGlobal requerido");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Meses.ToString(), "Meses", InformacionGlobal.Meses);
                }

                if (InformacionGlobal.Año < 2021)
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Año en InformacionGlobal incorrecto");
                }
            }

            return validationErrors;
        }
        private List<ValidationError> validateComprobanteCFDIRelacionados(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, BERecepcion.Core.Facturas.Dto.ComprobanteCfdiRelacionadosCfdiRelacionado[] CfdiRelacionados, string sourceDocument, bool esNotaCredito)
        {
            if (CfdiRelacionados != null)
            {
                pattern = getPattern(dataPattern, "Comprobante40.CFDIRelacionado.UUID");
                if (!string.IsNullOrEmpty(pattern))
                {
                    foreach (var it in CfdiRelacionados)
                    {
                        
                            Match me = Regex.Match(it.UUID, pattern, RegexOptions.IgnoreCase);
                            if (!me.Success)
                            {
                                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "UUID incorrecto para el CFDI relacionado");
                            }
                        
                    }
                }
                else
                {
                    Log.Error("Error de carga de patron para Comprobante40.CFDIRelacionado.UUID");
                }

            }

            return validationErrors;
        }
        private List<ValidationError> validateComprobanteEmisor(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, BERecepcion.Core.Facturas.Dto.ComprobanteEmisor Emisor, string sourceDocument, bool esNotaCredito)
        {
            if (Emisor == null)
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Emisor no especificado");
            }
            else
            {
                if (Emisor.Rfc == null)
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Emisor RFC no especificado");
                }
                if (String.IsNullOrEmpty(Emisor.RegimenFiscal))
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Emisor Regimen Fiscal no especificado");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_RegimenFiscal.ToString(), "EmisorRegimenFiscal", Emisor.RegimenFiscal);
                }
                if (!String.IsNullOrEmpty(Emisor.FacAtrAdquirente))
                {
                    pattern = getPattern(dataPattern, "Comprobante40.Emisor.FacAtrAdquirente");
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        Match me = Regex.Match(Emisor.FacAtrAdquirente, pattern, RegexOptions.IgnoreCase);
                        if (!me.Success)
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Emisor FacAtrAdquirente en formato incorrecto");
                        }
                    }
                    else
                    {
                        Log.Error("Error de carga de patron para Comprobante40.Emisor.FacAtrAdquirente");
                    }
                }
            }

            return validationErrors;
        }
        private List<ValidationError> validateComprobanteReceptor(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, BERecepcion.Core.Facturas.Dto.ComprobanteReceptor Receptor, string sourceDocument, bool esNotaCredito)
        {
            if (Receptor == null)
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Receptor requerido");
            }
            else
            {
                if (String.IsNullOrEmpty(Receptor.Rfc))
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Receptor RFC requerido");
                }

                if (String.IsNullOrEmpty(Receptor.Nombre))
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Receptor nombre requerido");
                }
                else
                {
                    if (Receptor.Nombre.Length > 254)
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Receptor nombre mayor a 254 caracteres");
                    }
                }

                if (string.IsNullOrEmpty(Receptor.DomicilioFiscalReceptor))
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "DomicilioFiscalReceptor requerido");
                }
                else
                {
                    if (Receptor.DomicilioFiscalReceptor.Length > 5)
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "DomicilioFiscalReceptor mayor a 5 caracteres");
                    }
                    else
                    {
                        pattern = getPattern(dataPattern, "Comprobante40.Receptor.DomicilioFiscalReceptor");
                        if (!string.IsNullOrEmpty(pattern))
                        {
                            Match me = Regex.Match(Receptor.DomicilioFiscalReceptor, pattern, RegexOptions.IgnoreCase);
                            if (!me.Success)
                            {
                                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "DomicilioFiscalReceptor en formato incorrecto");
                            }
                        }
                        else
                        {
                            Log.Error("Error de carga de patron para Comprobante40.Receptor.DomicilioFiscalReceptor");
                        }
                    }
                }

                if (Receptor.ResidenciaFiscalSpecified)
                {
                    if (String.IsNullOrEmpty(Receptor.ResidenciaFiscal))
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Receptor.ResidenciaFiscal requerida");
                    }
                    else
                    {
                        dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Pais.ToString(), "ReceptorResidenciaFiscal", Receptor.ResidenciaFiscal);
                    }
                }

                if (!String.IsNullOrEmpty(Receptor.NumRegIdTrib))
                {
                    if (Receptor.NumRegIdTrib.Length > 40)
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Receptor NumRegIdTrib longitud mayor a 40 caracteres");
                    }
                }

                if (String.IsNullOrEmpty(Receptor.RegimenFiscalReceptor))
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "RegimenFiscalReceptor requerido");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_RegimenFiscal.ToString(), "RegimenFiscalReceptor", Receptor.RegimenFiscalReceptor);
                }

                if (String.IsNullOrEmpty(Receptor.UsoCFDI))
                {
                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Receptor UsoCFDI requerido");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_UsoCFDI.ToString(), "UsoCFDI", Receptor.UsoCFDI);
                }
            }

            return validationErrors;
        }
        private List<ValidationError> validateComprobanteConceptos(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, BERecepcion.Core.Facturas.Dto.ComprobanteConcepto[] Conceptos, string sourceDocument, bool esNotaCredito)
        {
            if (Conceptos == null)
            {
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Conceptos requeridos");
            }
            else
            {
                foreach (var concepto in Conceptos)
                {
                    if (String.IsNullOrEmpty(concepto.ClaveProdServ))
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto " + concepto.Descripcion + " ClaveProdServ requerido");
                    }
                    else
                    {
                        dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ClaveProdServ.ToString(), "Concepto.ClaveProdServ", concepto.ClaveProdServ);
                    }
                    if (!String.IsNullOrEmpty(concepto.NoIdentificacion))
                    {
                        if (concepto.NoIdentificacion.Length > 100)
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto " + concepto.Descripcion + " NoIdentificacion mayor a 100 caracteres");
                        }
                    }
                    if (concepto.Cantidad == 0)
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Cantidad de un concepto en cero ( " + concepto.Descripcion + " ) no permitida");
                    }
                    if (String.IsNullOrEmpty(concepto.ClaveUnidad))
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto " + concepto.Descripcion + " ClaveUnidad requerida");
                    }
                    else
                    {
                        dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ClaveUnidad.ToString(), "ClaveUnidad", concepto.ClaveUnidad);
                    }
                    if (!String.IsNullOrEmpty(concepto.Unidad))
                    {
                        if (concepto.Unidad.Length > 20)
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto " + concepto.Descripcion + " Unidad longitud mayor a 20 caracteres");
                        }
                    }
                    if (!String.IsNullOrEmpty(concepto.Descripcion))
                    {
                        if (concepto.Descripcion.Length > 1000)
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto " + concepto.Descripcion + " descripcion mayor a 1000 caracteres");
                        }
                    }
                    if (concepto.ValorUnitario < 0)
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto " + concepto.Descripcion + " Valor unitario menor a cero no permitido");
                    }
                    if (concepto.Importe < 0)
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto " + concepto.Descripcion + " importe menor a cero no permitido");
                    }
                    if (concepto.DescuentoSpecified)
                    {
                        if (concepto.Descuento < 0)
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto " + concepto.Descripcion + " descuento menr a cero no permitido");
                        }
                    }
                    if (String.IsNullOrEmpty(concepto.ObjetoImp))
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto " + concepto.Descripcion + " ObjetoImp requerido");
                    }
                    else
                    {
                        dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ObjetoImp.ToString(), "ObjetoImp", concepto.ObjetoImp);
                    }
                    if (concepto.ACuentaTerceros != null)
                    {
                        if (String.IsNullOrEmpty(concepto.ACuentaTerceros.RfcACuentaTerceros))
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "RfcACuentaTerceros requerido");
                        }
                        if (String.IsNullOrEmpty(concepto.ACuentaTerceros.NombreACuentaTerceros))
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "NombreACuentaTerceros requerido");
                        }
                        else
                        {
                            if (concepto.ACuentaTerceros.NombreACuentaTerceros.Length > 254)
                            {
                                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "NombreACuentaTerceros mayor a 254 caracteres");
                            }
                        }
                        if (String.IsNullOrEmpty(concepto.ACuentaTerceros.RegimenFiscalACuentaTerceros))
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "RegimenFiscalACuentaTerceros requerido");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_RegimenFiscal.ToString(), "ACuentaTerceros.RegimenFiscalACuentaTerceros", concepto.ACuentaTerceros.RegimenFiscalACuentaTerceros);
                        }
                        if (String.IsNullOrEmpty(concepto.ACuentaTerceros.DomicilioFiscalACuentaTerceros))
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "DomicilioFiscalACuentaTerceros requerido");
                        }
                        else
                        {
                            pattern = getPattern(dataPattern, "Comprobante40.ACuentaTerceros.DomicilioFiscalACuentaTerceros");
                            if (!string.IsNullOrEmpty(pattern))
                            {
                                Match me = Regex.Match(concepto.ACuentaTerceros.DomicilioFiscalACuentaTerceros, pattern, RegexOptions.IgnoreCase);
                                if (!me.Success)
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "DomicilioFiscalACuentaTerceros en formato incorrecto");
                                }
                            }
                            else
                            {
                                Log.Error("Error de carga de patron para Comprobante40.ACuentaTerceros.DomicilioFiscalACuentaTerceros");
                            }
                        }
                    }
                    if (concepto.InformacionAduanera != null)
                    {
                        foreach (var item in concepto.InformacionAduanera)
                        {
                            if (String.IsNullOrEmpty(item.NumeroPedimento))
                            {
                                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "InformacionAduanera NumeroPedimento requerido");
                            }
                            else
                            {
                                pattern = getPattern(dataPattern, "Comprobante40.NumeroPedimento");
                                if (!string.IsNullOrEmpty(pattern))
                                {
                                    Match me = Regex.Match(item.NumeroPedimento, pattern, RegexOptions.IgnoreCase);
                                    if (!me.Success)
                                    {
                                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "InformacionAduanera NumeroPedimento en formato incorrecto");
                                    }
                                }
                                else
                                {
                                    Log.Error("Error de carga de patron para Comprobante40.NumeroPedimento");
                                }
                            }
                        }
                    }
                    if (concepto.CuentaPredial != null)
                    {
                        foreach (var item in concepto.CuentaPredial)
                        {
                            if (String.IsNullOrEmpty(item.Numero))
                            {
                                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "CuentaPredial Numero requerido");
                            }
                            else
                            {
                                pattern = getPattern(dataPattern, "Comprobante40.CuentaPredial.Numero");
                                if (!string.IsNullOrEmpty(pattern))
                                {
                                    Match me = Regex.Match(item.Numero, pattern, RegexOptions.IgnoreCase);
                                    if (!me.Success)
                                    {
                                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "CuentaPredial Numero en formato incorrecto");
                                    }
                                }
                                else
                                {
                                    Log.Error("Error de carga de patron para Comprobante40.CuentaPredial.Numero");
                                }
                            }
                        }
                    }
                    if (concepto.Parte != null)
                    {
                        foreach (var item in concepto.Parte)
                        {
                            if (String.IsNullOrEmpty(item.ClaveProdServ))
                            {
                                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto Parte ClaveProdServ requerido");
                            }
                            else
                            {
                                dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ClaveProdServ.ToString(), "Parte.ClaveProdServ", item.ClaveProdServ);
                            }
                            if (!String.IsNullOrEmpty(item.NoIdentificacion))
                            {
                                if (item.NoIdentificacion.Length > 100)
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto Parte NoIdentificacion mayor a 100 caracteres");
                                }
                            }
                            if (item.Cantidad <= 0)
                            {
                                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto Parte cantidad menor o igual a cero no permitida");
                            }
                            if (!String.IsNullOrEmpty(item.Unidad))
                            {
                                if (item.Unidad.Length > 20)
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto Parte Unidad mayor a 20 caracteres");
                                }
                            }
                            if (String.IsNullOrEmpty(item.Descripcion))
                            {
                                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto Parte descripcion requerida");
                            }
                            else
                            {
                                if (item.Descripcion.Length > 1000)
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto Parte descripcion mayor a 1000 caracteres");
                                }
                            }
                            if (item.ValorUnitarioSpecified)
                            {
                                if (item.ValorUnitario < 0)
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto Parte Valor Unitario menor a cero no permitido");
                                }
                            }
                            if (item.ImporteSpecified)
                            {
                                if (item.Importe < 0)
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Concepto Parte Importe menor o igual a cero no permitido");
                                }
                            }
                            if (item.InformacionAduanera != null)
                            {
                                foreach (var it in item.InformacionAduanera)
                                {
                                    if (String.IsNullOrEmpty(it.NumeroPedimento))
                                    {
                                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Parte InformacionAduanera NumeroPedimento en formato incorrecto");
                                    }
                                    else
                                    {
                                        pattern = getPattern(dataPattern, "Comprobante40.NumeroPedimento");
                                        if (!string.IsNullOrEmpty(pattern))
                                        {
                                            Match me = Regex.Match(it.NumeroPedimento, pattern, RegexOptions.IgnoreCase);
                                            if (!me.Success)
                                            {
                                                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Parte InformacionAduanera NumeroPedimento en formato incorrecto");
                                            }
                                        }
                                        else
                                        {
                                            Log.Error("Error de carga de patron para Comprobante40.NumeroPedimento");
                                        }
                                    }

                                }
                            }
                        }
                    }
                    if(concepto.Impuestos!=null)
                    {
                        if(concepto.Impuestos.Retenciones!=null)
                        {
                            foreach (var retencion in concepto.Impuestos.Retenciones)
                            {
                                if(retencion.Base<0)
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Retencion base menor a cero no permitido");
                                }
                                if (String.IsNullOrEmpty(retencion.Impuesto))
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Retencion atributo [Impuesto] requerido");
                                }
                                else
                                {
                                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Impuesto.ToString(), "Retencion.Impuesto", retencion.Impuesto);
                                }
                                if (String.IsNullOrEmpty(retencion.TipoFactor))
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Retencion TipoFactor requerido");
                                }
                                else
                                {
                                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoFactor.ToString(), "Retencion.TipoFactor", retencion.TipoFactor);
                                }
                                //requerido cuando tipofactor tenga una clave que corresponda a tasa o cuota
                                if (retencion.TasaOCuota < 0)
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Retencion TasaOCuota menor a cero no permitido");
                                }
                                if (retencion.Importe < 0)
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Retencion Importe menor a cero no permitido");
                                }

                            }
                        }
                        if(concepto.Impuestos.Traslados!=null)
                        {
                            foreach (var traslado in concepto.Impuestos.Traslados)
                            {
                                if (traslado.Base < 0)
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Traslado base menor a cero no permitido");
                                }
                                if (String.IsNullOrEmpty(traslado.Impuesto))
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Traslado atributo [impuesto] requerido");
                                }
                                else
                                {
                                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Impuesto.ToString(), "Traslado.Impuesto", traslado.Impuesto);
                                }
                                if (String.IsNullOrEmpty(traslado.TipoFactor))
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Traslado TipoFactor requerido");
                                }
                                else
                                {
                                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoFactor.ToString(), "Traslado.TipoFactor", traslado.TipoFactor);
                                }
                                //requerido cuando tipofactor tenga una clave que corresponda a tasa o cuota
                                if (traslado.TasaOCuota < 0)
                                {
                                    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Traslado TasaOCuota menor a cero no permitido");
                                }
                                if (traslado.ImporteSpecified)
                                {
                                    if (traslado.Importe < 0)
                                    {
                                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Traslado Importe menor a cero no permitido");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return validationErrors;
        }
        private List<ValidationError> validateComprobanteImpuestos(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, BERecepcion.Core.Facturas.Dto.ComprobanteImpuestos Impuestos, string sourceDocument, bool esNotaCredito)
        {
            if (Impuestos != null)
            {
                if (Impuestos.Traslados != null)
                {
                    foreach (var traslado in Impuestos.Traslados)
                    {
                        if (traslado.Base < 0)
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Traslado base menor a cero no permitido");
                        }
                        if (String.IsNullOrEmpty(traslado.Impuesto))
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Traslado atributo [impuesto] requerido");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Impuesto.ToString(), "Traslado.Impuesto", traslado.Impuesto);
                        }
                        if (String.IsNullOrEmpty(traslado.TipoFactor))
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Traslado TipoFactor requerido");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoFactor.ToString(), "Traslado.TipoFactor", traslado.TipoFactor);
                        }
                        //requerido cuando tipofactor tenga una clave que corresponda a tasa o cuota
                        if (traslado.TasaOCuota < 0)
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Traslado TasaOCuota menor a cero no permitido");
                        }
                        if (traslado.ImporteSpecified)
                        {
                            if (traslado.Importe < 0)
                            {
                                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Traslado Importe menor a cero no permitido");
                            }
                        }
                    }
                }
                if (Impuestos.Retenciones != null)
                {
                    foreach (var retencion in Impuestos.Retenciones)
                    {
                        //faltante
                        //if(retencion.Base<0)
                        //{
                        //    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Retencion base menor a cero no permitido");
                        //}
                        if (String.IsNullOrEmpty(retencion.Impuesto))
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Retencion atributo [Impuesto] requerido");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Impuesto.ToString(), "Retencion.Impuesto", retencion.Impuesto);
                        }
                        //faltante
                        //if (String.IsNullOrEmpty(retencion.TipoFactor))
                        //{
                        //    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Retencion TipoFactor requerido");
                        //}
                        //else
                        //{
                        //    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoFactor.ToString(), "Retencion.TipoFactor", retencion.TipoFactor);
                        //}
                        //faltante
                        //requerido cuando tipofactor tenga una clave que corresponda a tasa o cuota
                        //if (retencion.TasaOCuota < 0)
                        //{
                        //    validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Retencion TasaOCuota menor a cero no permitido");
                        //}
                        if (retencion.Importe < 0)
                        {
                            validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Retencion Importe menor a cero no permitido");
                        }

                    }
                }
                if (Impuestos.TotalImpuestosRetenidosSpecified)
                {
                    if (Impuestos.TotalImpuestosRetenidos < 0)
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "TotalImpuestosRetenidos menor a cero no permitido");
                    }
                }
                if (Impuestos.TotalImpuestosTrasladadosSpecified)
                {
                    if (Impuestos.TotalImpuestosTrasladados < 0)
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "TotalImpuestosTrasladados menor a cero no permitido");
                    }
                }

            }
            return validationErrors;
        }
        public async Task<List<ValidationError>> validateComprobante(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, ComprobanteBE40 comprobante, string sourceDocument, bool esNotaCredito)
        {
            dataSearch = new List<DataSearch>() { };
            dataPattern = new List<DataSearchRegExp>() { };

            try
            {
                dataPattern = await loadDataPattern();

                validationErrors = validateComprobanteHdr(errorCatalog, validationErrors, comprobante, sourceDocument, esNotaCredito);
                validationErrors = validateComprobanteInformacionGlobal(errorCatalog, validationErrors, comprobante.InformacionGlobal, sourceDocument, esNotaCredito);
                validationErrors = validateComprobanteCFDIRelacionados(errorCatalog, validationErrors, comprobante.CfdiRelacionados, sourceDocument, esNotaCredito);
                validationErrors = validateComprobanteEmisor(errorCatalog, validationErrors, comprobante.Emisor, sourceDocument, esNotaCredito);
                validationErrors = validateComprobanteReceptor(errorCatalog, validationErrors, comprobante.Receptor, sourceDocument, esNotaCredito);
                validationErrors = validateComprobanteConceptos(errorCatalog, validationErrors, comprobante.Conceptos, sourceDocument, esNotaCredito);
                validationErrors = validateComprobanteImpuestos(errorCatalog, validationErrors, comprobante.Impuestos, sourceDocument, esNotaCredito);

                if (dataSearch.Count > 0)
                {
                    validationErrors = await checkDBAsync(dataSearch, esNotaCredito, errorCatalog, validationErrors, sourceDocument);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return validationErrors;
        }

        private List<ValidationError> addErrorComp(bool esNotaCredito, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, string clave, string claveNC, string documento, string descripcion)
        {
            var itR = catalogErrors.Where(x => x.clave == (!esNotaCredito ? clave : claveNC)).FirstOrDefault();

            if (itR != null)
            {
                ValidationError item = new ValidationError();
                item.id = itR.id;
                item.activo = itR.activo;
                item.clave = itR.clave;
                item.esTerminal = itR.esTerminal;
                validationErrors.Add(item);

                validationErrors[validationErrors.Count - 1].documento = documento;
                if (!string.IsNullOrEmpty(descripcion))
                {
                    validationErrors[validationErrors.Count - 1].descripcion = descripcion;
                    validationErrors[validationErrors.Count - 1].mensaje = descripcion;
                }
            }

            return validationErrors;
        }

        private List<DataSearch> itemSearch(List<DataSearch> lSearch, string table, string field, string value)
        {
            lSearch.Add(new DataSearch() { tabla = table, campo = field, dato = value });
            return lSearch;
        }

        private async Task<List<ValidationError>> checkDBAsync(List<DataSearch> dataSearch, bool esNotaCredito, IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, string sourceDocument)
        {
            var dt = new DataTable("dbo.CartaPorteData");
            dt.Columns.Add("tabla");
            dt.Columns.Add("campo");
            dt.Columns.Add("dato");

            foreach (var it in dataSearch)
            {
                dt.Rows.Add(it.tabla, it.campo, it.dato);
            }

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    var cg = await db.QueryAsync<DataSearch>(sql: "SP_CartaPorte_tabla_seleccion", new { Data = dt }, commandType: CommandType.StoredProcedure);
                    if (cg == null)
                    {
                        validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Error al consultar base para datos de CartaPorte");
                    }
                    else
                    {
                        foreach (var it in dataSearch)
                        {
                            var r = cg.FirstOrDefault(x => x.tabla == it.tabla && x.campo == it.campo && x.dato == it.dato);
                            if (r == null)
                            {
                                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Campo (" + it.campo + ") con el valor (" + it.dato.ToString() + ") incorrecto segun catalogos del SAT");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                validationErrors = addErrorComp(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Error al consultar base para datos de CartaPorte");
            }

            return validationErrors;
        }

        private async Task<List<DataSearchRegExp>> loadDataPattern()
        {
            List<DataSearchRegExp> result = new List<DataSearchRegExp>();

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    var r = await db.QueryAsync<DataSearchRegExp>(sql: "SP_CartaPorte_expresiones_seleccion", commandType: CommandType.StoredProcedure);
                    if (r != null)
                    {
                        result = r.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return result;
        }

        private string getPattern(List<DataSearchRegExp> dataPattern, string pattern)
        {
            string result = "";

            if (dataPattern != null)
            {
                var r = dataPattern.Where(x => x.campo == pattern).FirstOrDefault();
                if (r != null)
                {
                    result = @r.patron;
                }
            }

            return result;
        }

    }
}
