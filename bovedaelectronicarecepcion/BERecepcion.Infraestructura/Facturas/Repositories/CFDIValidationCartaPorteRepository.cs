using BERecepcion.Core.Models;
using System;
using System.Collections.Generic;
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
using BERecepcion.Infraestructura.Utils;

namespace BERecepcion.Infraestructura.Facturas.Repositories
{
    public class CFDIValidationCartaPorteRepository : BaseSQLServerSqlRepository, ICFDIValidationCartaPorteRepository
    {
        public CFDIValidationCartaPorteRepository(string cnnString) : base(cnnString)
        {
        }

        string pattern = "";
        List<DataSearch> dataSearch = null;
        List<DataSearchRegExp> dataPattern = null;

        private List<ValidationError> CartaPorteHdr(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, CartaPorte cartaPorte, string sourceDocument, bool esNotaCredito)
        {
            if (string.IsNullOrEmpty(cartaPorte.Version))
            {
                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Version requerida");
            }

            if (cartaPorte.EntradaSalidaMercSpecified)
            {
                if (string.IsNullOrEmpty(cartaPorte.EntradaSalidaMerc))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "No se especificó si es entrada o salida de mercancías (EntradaSalidaMerc)");
                }
                else
                {
                    if (!(cartaPorte.EntradaSalidaMerc.ToUpper() == "ENTRADA" ||
                        cartaPorte.EntradaSalidaMerc.ToUpper() == "SALIDA"))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Valor incorrecto para entrada o salida de mercancías (EntradaSalidaMerc)");
                    }
                }
            }

            if (cartaPorte.PaisOrigenDestinoSpecified)
            {
                if (string.IsNullOrEmpty(cartaPorte.PaisOrigenDestino))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Clave del pais de origen o destino no especificada (PaisOrigenDestino)");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Pais.ToString(), "PaisOrigenDestino", cartaPorte.PaisOrigenDestino);
                }
            }

            if (cartaPorte.TotalDistRecSpecified)
            {
                if (cartaPorte.TotalDistRec == 0)
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Distancia recorrida por los productos no especificada (TotalDistRec)");
                }
            }

            if (cartaPorte.ViaEntradaSalidaSpecified)
            {
                if (string.IsNullOrEmpty(cartaPorte.ViaEntradaSalida))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "No se especificó la via de entrada o salida de los bienes / servicios");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_CveTransporte.ToString(), "ViaEntradaSalida", cartaPorte.ViaEntradaSalida);
                }
            }

            if (string.IsNullOrEmpty(cartaPorte.TranspInternac))
            {
                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Atributo de transporte internacional (TranspInternac) requerido");
            }

            if (cartaPorte.FiguraTransporte != null)
            {
                foreach (var ft in cartaPorte.FiguraTransporte)
                {
                    if (ft.Domicilio != null)
                    {
                        //opcional Calle
                        if (string.IsNullOrEmpty(ft.Domicilio.Calle))
                        {
                            //sin operacion
                        }
                        if (string.IsNullOrEmpty(ft.Domicilio.CodigoPostal))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Codigo postal figura transporte requerido");
                        }
                        //opcional Colonia
                        if (string.IsNullOrEmpty(ft.Domicilio.Colonia))
                        {
                            //sin operacion
                        }
                        if (string.IsNullOrEmpty(ft.Domicilio.Estado))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Estado en figura transporte requerido");
                        }
                        //opcional Localidad
                        if (string.IsNullOrEmpty(ft.Domicilio.Localidad))
                        {
                            //sin operacion
                        }
                        //opcional Municipio
                        if (string.IsNullOrEmpty(ft.Domicilio.Municipio))
                        {
                            //sin operacion
                        }
                        //opcional NumeroExterior
                        if (string.IsNullOrEmpty(ft.Domicilio.NumeroExterior))
                        {
                            //sin operacion
                        }
                        //opcional NumeroInterior
                        if (string.IsNullOrEmpty(ft.Domicilio.NumeroInterior))
                        {
                            //sin operacion
                        }
                        if (string.IsNullOrEmpty(ft.Domicilio.Pais))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Clave del pais figura transporte requerido");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Pais.ToString(), "Pais", ft.Domicilio.Pais);
                        }
                        //opcional Referencia
                        if (string.IsNullOrEmpty(ft.Domicilio.Referencia))
                        {
                            //sin operacion
                        }
                    }
                    if (!string.IsNullOrEmpty(ft.NombreFigura))
                    {
                        if (ft.NombreFigura.Length > 254)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud nombre figura transporte mayor a 254 caracteres");
                        }
                    }
                    if (!string.IsNullOrEmpty(ft.NumLicencia))
                    {
                        if (ft.NumLicencia.Length > 16)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud numero licencia figura transporte mayor a 16 caracteres");
                        }
                    }
                    if (!string.IsNullOrEmpty(ft.NumRegIdTribFigura))
                    {
                        if (ft.NumRegIdTribFigura.Length < 6 ||
                            ft.NumRegIdTribFigura.Length > 40)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud de la identificacion tributaria de la figura de transporte incorrecta (NumRegIdTribFigura)");
                        }
                    }
                    if (ft.PartesTransporte != null)
                    {
                        foreach (var pt in ft.PartesTransporte)
                        {
                            if (string.IsNullOrEmpty(pt.ParteTransporte))
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Parte transporte requerido");
                            }
                            else
                            {
                                dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ParteTransporte.ToString(), "ParteTransporte", pt.ParteTransporte);
                            }
                        }
                    }
                    if (ft.ResidenciaFiscalFiguraSpecified)
                    {
                        if (!string.IsNullOrEmpty(ft.ResidenciaFiscalFigura))
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Pais.ToString(), "ResidenciaFiscalFigura", ft.ResidenciaFiscalFigura);
                        }
                    }
                    if (!string.IsNullOrEmpty(ft.RFCFigura))
                    {
                        //sin operacion
                    }
                    if (string.IsNullOrEmpty(ft.TipoFigura))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo figura transporte requerido");
                    }
                    else
                    {

                        dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_FiguraTransporte.ToString(), "TipoFigura", ft.TipoFigura);
                    }
                }
            }

            return validationErrors;
        }

        private List<ValidationError> CartaPorteAutoTransporte(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, CartaPorte cartaPorte, string sourceDocument, bool esNotaCredito)
        {
            if (cartaPorte.Mercancias.Autotransporte != null)
            {
                if (cartaPorte.Mercancias.Autotransporte.IdentificacionVehicular != null)
                {
                    if (cartaPorte.Mercancias.Autotransporte.IdentificacionVehicular.AnioModeloVM == 0)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "No se especificó el año del modelo en la identificacion vehicular");
                    }
                    else
                    {
                        pattern = getPattern(dataPattern, "IdentificacionVehicular.AnioModeloVM");
                        if (!string.IsNullOrEmpty(pattern))
                        {
                            Match me = Regex.Match(cartaPorte.Mercancias.Autotransporte.IdentificacionVehicular.AnioModeloVM.ToString(), pattern, RegexOptions.IgnoreCase);
                            if (!me.Success)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Año vehicular incorrecto (AnioModeloVM)");
                            }
                        }
                        else
                        {
                            Log.Error("Error de carga de patron para CartaPorteRepository IdentificacionVehicular.AnioModeloVM");
                        }
                    }
                    if (string.IsNullOrEmpty(cartaPorte.Mercancias.Autotransporte.IdentificacionVehicular.ConfigVehicular))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "No se especificó clave de nomenclatura del autotransporte (ConfigVehicular)");
                    }
                    else
                    {
                        dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ConfigAutotransporte.ToString(), "ConfigVehicular", cartaPorte.Mercancias.Autotransporte.IdentificacionVehicular.ConfigVehicular);
                    }
                    if (string.IsNullOrEmpty(cartaPorte.Mercancias.Autotransporte.IdentificacionVehicular.PlacaVM))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Placa vehicular no especificada");
                    }
                    else
                    {
                        pattern = getPattern(dataPattern, "IdentificacionVehicular.PlacaVM");
                        if (!string.IsNullOrEmpty(pattern))
                        {
                            Match me = Regex.Match(cartaPorte.Mercancias.Autotransporte.IdentificacionVehicular.PlacaVM, pattern, RegexOptions.IgnoreCase);
                            if (!me.Success)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Formato de placa vehicular incorrecto (PlacaVM)");
                            }
                        }
                        else
                        {
                            Log.Error("Error de carga de patron para CartaPorteRepository IdentificacionVehicular.PlacaVM");
                        }
                    }
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.Autotransporte.NumPermisoSCT))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero de permiso SCT requerido (NumPermisoSCT)");
                }
                else
                {
                    if (cartaPorte.Mercancias.Autotransporte.NumPermisoSCT.Length > 50)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud del permiso SCT mayor que la permitida (NumPermisoSCT)");
                    }
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.Autotransporte.PermSCT))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Clave del tipo de permiso SCT requerido (PermSCT)");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoPermiso.ToString(), "PermSCT", cartaPorte.Mercancias.Autotransporte.PermSCT);
                }
                if (cartaPorte.Mercancias.Autotransporte.Remolques != null)
                {
                    foreach (var rem in cartaPorte.Mercancias.Autotransporte.Remolques)
                    {
                        if (string.IsNullOrEmpty(rem.Placa))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Placa de remolque no especificada");
                        }
                        else
                        {
                            pattern = getPattern(dataPattern, "Remolques.Placa");
                            if (!string.IsNullOrEmpty(pattern))
                            {
                                Match me = Regex.Match(rem.Placa, pattern, RegexOptions.IgnoreCase);
                                if (!me.Success)
                                {
                                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Formato de placa de remolque incorrecto");
                                }
                            }
                            else
                            {
                                Log.Error("Error de carga de patron para CartaPorteRepository Remolques.Placa");
                            }
                        }
                        if (string.IsNullOrEmpty(rem.SubTipoRem))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Subtipo de remolque no especificado (SubTipoRem)");
                        }
                    }
                }
                if (cartaPorte.Mercancias.Autotransporte.Seguros != null)
                {
                    if (!string.IsNullOrEmpty(cartaPorte.Mercancias.Autotransporte.Seguros.AseguraCarga))
                    {
                        if (cartaPorte.Mercancias.Autotransporte.Seguros.AseguraCarga.Length > 50)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Nombre de la aseguradora para poliza de carga mayor a 50 caracteres");
                        }
                    }
                    if (!string.IsNullOrEmpty(cartaPorte.Mercancias.Autotransporte.Seguros.AseguraMedAmbiente))
                    {
                        if (cartaPorte.Mercancias.Autotransporte.Seguros.AseguraMedAmbiente.Length > 50)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Nombre de la aseguradora para poliza de medio ambiente mayor a 50 caracteres");
                        }
                    }
                    if (string.IsNullOrEmpty(cartaPorte.Mercancias.Autotransporte.Seguros.AseguraRespCivil))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "No se especifico el nombre de la aseguradora del transporte");
                    }
                    else
                    {
                        if (cartaPorte.Mercancias.Autotransporte.Seguros.AseguraRespCivil.Length > 50)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Nombre de la asegura del transporte mayor a 50 caracteres");
                        }
                    }
                    if (!string.IsNullOrEmpty(cartaPorte.Mercancias.Autotransporte.Seguros.PolizaCarga))
                    {
                        if (cartaPorte.Mercancias.Autotransporte.Seguros.PolizaCarga.Length > 30)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero de poliza de carga mayor a 30 caracteres");
                        }
                    }
                    if (!string.IsNullOrEmpty(cartaPorte.Mercancias.Autotransporte.Seguros.PolizaMedAmbiente))
                    {
                        if (cartaPorte.Mercancias.Autotransporte.Seguros.PolizaMedAmbiente.Length > 30)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero de poliza de medio ambiente mayor a 30 caracteres");
                        }
                    }
                    if (string.IsNullOrEmpty(cartaPorte.Mercancias.Autotransporte.Seguros.PolizaRespCivil))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "No se especificó el numero de poliza de responsabilidad civil (PolizaRespCivil)");
                    }
                    else
                    {
                        if (cartaPorte.Mercancias.Autotransporte.Seguros.PolizaRespCivil.Length > 30)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud del numero de poliza de responsabilidad civil mayor a 30 caracteres (PolizaRespCivil)");
                        }
                    }
                    if (cartaPorte.Mercancias.Autotransporte.Seguros.PrimaSeguroSpecified)
                    {
                        //opcional PrimaSeguro
                        if (cartaPorte.Mercancias.Autotransporte.Seguros.PrimaSeguro == 0)
                        {
                            //sin operacion
                        }
                    }
                }
            }

            return validationErrors;
        }

        private List<ValidationError> CartaPorteTransporteAereo(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, CartaPorte cartaPorte, string sourceDocument, bool esNotaCredito)
        {
            if (cartaPorte.Mercancias.TransporteAereo != null)
            {
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.CodigoTransportista))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Codigo transportista aereo requerido");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_CodigoTransporteAereo.ToString(), "CodigoTransportista", cartaPorte.Mercancias.TransporteAereo.CodigoTransportista);
                }
                //opcional LugarContrato
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.LugarContrato))
                {
                    //sin operacion
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.MatriculaAeronave))
                {
                    pattern = getPattern(dataPattern, "TransporteAereo.MatriculaAeronave");
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        Match me = Regex.Match(cartaPorte.Mercancias.TransporteAereo.MatriculaAeronave, pattern, RegexOptions.IgnoreCase);
                        if (!me.Success)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Formato de matricula aeronave incorrecto");
                        }
                    }
                    else
                    {
                        Log.Error("Error de carga de patron para CartaPorteRepository TransporteAereo.MatriculaAeronave");
                    }
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.NombreAseg))
                {
                    if (cartaPorte.Mercancias.TransporteAereo.NombreAseg.Length > 50)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Nombre aseguradora transporte aereo mayor a 50 caracteres");
                    }
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.NombreEmbarcador)) { }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.NumeroGuia))
                {
                    if (cartaPorte.Mercancias.TransporteAereo.NumeroGuia.Length < 12 ||
                        cartaPorte.Mercancias.TransporteAereo.NumeroGuia.Length > 15)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud numero de guia transporte aereo incorrecta");
                    }
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.NumPermisoSCT))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero permiso SCT para transporte aereo requerido (NumPermisoSCT)");
                }
                else
                {
                    if (cartaPorte.Mercancias.TransporteAereo.NumPermisoSCT.Length > 50)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud del permiso SCT incorrecta");
                    }
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.NumPolizaSeguro))
                {
                    if (cartaPorte.Mercancias.TransporteAereo.NumPolizaSeguro.Length > 30)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud poliza de seguro transporte aereo mayor a 30 caracteres");
                    }
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.NumRegIdTribEmbarc))
                {
                    if (cartaPorte.Mercancias.TransporteAereo.NumRegIdTribEmbarc.Length < 6 ||
                        cartaPorte.Mercancias.TransporteAereo.NumRegIdTribEmbarc.Length > 40)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud del registro fiscal del embarcador incorrecta (NumRegIdTribEmbarc)");
                    }
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.PermSCT))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo Permiso SCT para transporte aereo requerido (PermSCT)");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoPermiso.ToString(), "PermSCT", cartaPorte.Mercancias.TransporteAereo.PermSCT);
                }
                if (cartaPorte.Mercancias.TransporteAereo.ResidenciaFiscalEmbarcSpecified)
                {
                    if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.ResidenciaFiscalEmbarc))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Residencia fiscal embarcador transporte aereo no especificada (ResidenciaFiscalEmbarc)");
                    }
                    else
                    {
                        dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Pais.ToString(), "ResidenciaFiscalEmbarc", cartaPorte.Mercancias.TransporteAereo.ResidenciaFiscalEmbarc);
                    }
                }
                //opcional RFCEmbarcador
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteAereo.RFCEmbarcador))
                {
                    //sin operacion
                }
            }

            return validationErrors;
        }

        private List<ValidationError> CartaPorteTransporteFerroviario(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, CartaPorte cartaPorte, string sourceDocument, bool esNotaCredito)
        {
            if (cartaPorte.Mercancias.TransporteFerroviario != null)
            {
                if (cartaPorte.Mercancias.TransporteFerroviario.Carro != null)
                {
                    foreach (var cr in cartaPorte.Mercancias.TransporteFerroviario.Carro)
                    {
                        if (cr.Contenedor != null)
                        {
                            foreach (var ct in cr.Contenedor)
                            {
                                if (ct.PesoContenedorVacio == 0)
                                {
                                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Peso contenedor vacio en cero transporte ferroviario no permitido");
                                }
                                if (ct.PesoNetoMercancia == 0)
                                {
                                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Peso neto mercancia en cero transporte ferroviario no permitido");
                                }
                                if (string.IsNullOrEmpty(ct.TipoContenedor))
                                {
                                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo contenedor transporte ferroviario requerido");
                                }
                                else
                                {
                                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Contenedor.ToString(), "TipoContenedor", ct.TipoContenedor);
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(cr.GuiaCarro))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Guia carro transporte ferroviario requerida");
                        }
                        else
                        {
                            if (cr.GuiaCarro.Length > 15)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud de la guia del carro transporte ferroviario mayor a 15 caracteres");
                            }
                        }
                        if (string.IsNullOrEmpty(cr.MatriculaCarro))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Matricula carro transporte ferroviario requerida");
                        }
                        else
                        {
                            if (cr.MatriculaCarro.Length < 6 ||
                                cr.MatriculaCarro.Length > 15)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud de matricula del carro transporte ferroviario incorrecta");
                            }
                        }
                        if (string.IsNullOrEmpty(cr.TipoCarro))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo de carro transporte ferroviario requerido");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoCarro.ToString(), "TipoCarro", cr.TipoCarro);
                        }
                        if (cr.ToneladasNetasCarro == 0)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Toneladas carro en cero transporte ferroviario no permitido");
                        }
                    }
                }
                if (cartaPorte.Mercancias.TransporteFerroviario.DerechosDePaso != null)
                {
                    foreach (var dp in cartaPorte.Mercancias.TransporteFerroviario.DerechosDePaso)
                    {
                        if (dp.KilometrajePagado == 0)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "kilometraje pagado en cero transporte ferroviario no permitido");
                        }
                        if (string.IsNullOrEmpty(dp.TipoDerechoDePaso))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo de derecho de paso transporte ferroviario requerido");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_DerechosDePaso.ToString(), "TipoDerechoDePaso", dp.TipoDerechoDePaso);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteFerroviario.NombreAseg))
                {
                    if (cartaPorte.Mercancias.TransporteFerroviario.NombreAseg.Length > 50)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud nombre aseguradora transporte ferroviario mayor a 50 caracteres");
                    }
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteFerroviario.NumPolizaSeguro))
                {
                    if (cartaPorte.Mercancias.TransporteFerroviario.NumPolizaSeguro.Length < 3 ||
                        cartaPorte.Mercancias.TransporteFerroviario.NumPolizaSeguro.Length > 30)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud de la poliza de seguro transporte ferroviario diferente a la permitida");
                    }
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteFerroviario.TipoDeServicio))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo de servicio para transporte ferroviario requerido");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoDeServicio.ToString(), "TipoDeServicio", cartaPorte.Mercancias.TransporteFerroviario.TipoDeServicio);
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteFerroviario.TipoDeTrafico))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo de trafico ferroviario requerido");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoDeTrafico.ToString(), "TipoDeTrafico", cartaPorte.Mercancias.TransporteFerroviario.TipoDeTrafico);
                }
            }

            return validationErrors;
        }

        private List<ValidationError> CartaPorteTransporteMaritimo(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, CartaPorte cartaPorte, string sourceDocument, bool esNotaCredito)
        {
            if (cartaPorte.Mercancias.TransporteMaritimo != null)
            {
                if (cartaPorte.Mercancias.TransporteMaritimo.AnioEmbarcacionSpecified)
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.AnioEmbarcacion == 0)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Año de la embarcacion incorrecto");
                    }
                    else
                    {
                        pattern = getPattern(dataPattern, "TransporteMaritimo.AnioEmbarcacion");         //?
                        if (!string.IsNullOrEmpty(pattern))
                        {
                            Match me = Regex.Match(cartaPorte.Mercancias.TransporteMaritimo.AnioEmbarcacion.ToString(), pattern, RegexOptions.IgnoreCase);
                            if (!me.Success)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Año de la embarcacion incorrecto");
                            }
                        }
                        else
                        {
                            Log.Error("Error de carga de patron para CartaPorteRepository TransporteMaritimo.AnioEmbarcacion");
                        }
                    }
                }
                if (cartaPorte.Mercancias.TransporteMaritimo.CaladoSpecified)
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.Calado == 0)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Valor de calado en cero no permitido");
                    }
                }
                if (cartaPorte.Mercancias.TransporteMaritimo.Contenedor != null)
                {
                    foreach (var ctr in cartaPorte.Mercancias.TransporteMaritimo.Contenedor)
                    {
                        if (string.IsNullOrEmpty(ctr.MatriculaContenedor))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Matricula contenedor para transporte maritimo requerida");
                        }
                        else
                        {
                            if (ctr.MatriculaContenedor.Length < 11 || ctr.MatriculaContenedor.Length > 15)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud matricula contenedor transporte maritimo incorrecta");
                            }
                        }
                        if (!string.IsNullOrEmpty(ctr.NumPrecinto))
                        {
                            if (ctr.NumPrecinto.Length < 5 || ctr.NumPrecinto.Length > 20)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud numero de precinto para transporte maritimo incorrecta");
                            }
                        }
                        if (string.IsNullOrEmpty(ctr.TipoContenedor))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo contenedor maritimo requerido");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ContenedorMaritimo.ToString(), "TipoContenedor", ctr.TipoContenedor);
                        }
                    }
                }
                if (cartaPorte.Mercancias.TransporteMaritimo.EsloraSpecified)
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.Eslora == 0)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Valor de eslora en cero no permitido");
                    }
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.LineaNaviera))
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.LineaNaviera.Length < 3 ||
                        cartaPorte.Mercancias.TransporteMaritimo.LineaNaviera.Length > 50)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud de linea naviera menor a 3 caracteres o mayor a 50");
                    }
                }
                if (cartaPorte.Mercancias.TransporteMaritimo.MangaSpecified)
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.Manga == 0)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Valor de manga en cero no permitido");
                    }
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.Matricula))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Matricula de embarcacion no especificada");
                }
                else
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.Matricula.Length < 7 || cartaPorte.Mercancias.TransporteMaritimo.Matricula.Length > 30)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Matricula de la embarcacion menor a 7 caracteres o mayor a 30");
                    }
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.NacionalidadEmbarc))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Nacionalidad de la embarcacion requerida");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Pais.ToString(), "NacionalidadEmbarc", cartaPorte.Mercancias.TransporteMaritimo.NacionalidadEmbarc);
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.NombreAgenteNaviero))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Nombre de agente naviero requerido");
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.NombreAseg))
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.NombreAseg.Length > 50)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Nombre de la aseguradora para transporte maritimo mayor a 50 caracteres");
                    }
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.NombreEmbarc))
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.NombreEmbarc.Length > 50)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud del nombre de embarcacion mayor a 50 caracteres");
                    }
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.NumAutorizacionNaviero))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero autorizacion naviero requerido");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_NumAutorizacionNaviero.ToString(), "NumAutorizacionNaviero", cartaPorte.Mercancias.TransporteMaritimo.NumAutorizacionNaviero);
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.NumCertITC))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Certificado ITC requerido");
                }
                else
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.NumCertITC.Length < 3 ||
                        cartaPorte.Mercancias.TransporteMaritimo.NumCertITC.Length > 20)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud del certificado ITC incorrecta");
                    }
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.NumConocEmbarc))
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.NumConocEmbarc.Length > 30)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero de conocimiento de embarque mayor a 30 caracteres");
                    }
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.NumeroOMI))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero OMI requerido");
                }
                else
                {
                    pattern = getPattern(dataPattern, "TransporteMaritimo.NumeroOMI");
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        Match me = Regex.Match(cartaPorte.Mercancias.TransporteMaritimo.NumeroOMI, pattern, RegexOptions.IgnoreCase);
                        if (!me.Success)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Formato de numero OMI incorrecto");
                        }
                    }
                    else
                    {
                        Log.Error("Error de carga de patron para CartaPorteRepository TransporteMaritimo.NumeroOMI");
                    }
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.NumPermisoSCT))
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.NumPermisoSCT.Length > 30)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero de Permiso SCT maritimo mayor a 30 caracteres");
                    }
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.NumPolizaSeguro))
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.NumPolizaSeguro.Length > 30)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero de poliza de seguro maritimo mayor a 30 caracteres");
                    }
                }
                if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.NumViaje))
                {
                    if (cartaPorte.Mercancias.TransporteMaritimo.NumViaje.Length > 30)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero de viaje maritimo mayor a 30 caracteres");
                    }
                }
                if (cartaPorte.Mercancias.TransporteMaritimo.PermSCTSpecified)
                {
                    if (!string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.PermSCT))
                    {
                        dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoPermiso.ToString(), "PermSCT", cartaPorte.Mercancias.TransporteMaritimo.PermSCT);
                    }
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.TipoCarga))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo de carga en transporte maritimo no especificado");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ClaveTipoCarga.ToString(), "TipoCarga", cartaPorte.Mercancias.TransporteMaritimo.TipoCarga);
                }
                if (string.IsNullOrEmpty(cartaPorte.Mercancias.TransporteMaritimo.TipoEmbarcacion))
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo de embarcacion no especificada (TipoEmbarcacion)");
                }
                else
                {
                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ConfigMaritima.ToString(), "TipoEmbarcacion", cartaPorte.Mercancias.TransporteMaritimo.TipoEmbarcacion);
                }
                if (cartaPorte.Mercancias.TransporteMaritimo.UnidadesDeArqBruto == 0)
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Unidades de Arqueo Bruto en cero no permitido");
                }
            }

            return validationErrors;
        }

        private List<ValidationError> CartaPorteMercancia(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, CartaPorte cartaPorte, string sourceDocument, bool esNotaCredito)
        {
            if (cartaPorte.Mercancias.CargoPorTasacionSpecified)
            {
                //opcional CargoPorTasacion
                if (cartaPorte.Mercancias.CargoPorTasacion == 0)
                {
                    //sin operacion
                }
            }
            if (cartaPorte.Mercancias.NumTotalMercancias <= 0)
            {
                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero total de mercancias incorrecto (NumTotalMercancias)");
            }
            if (cartaPorte.Mercancias.PesoBrutoTotal == 0)
            {
                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Peso bruto total incorrecto (PesoBrutoTotal)");
            }
            if (cartaPorte.Mercancias.PesoNetoTotalSpecified)
            {
                if (cartaPorte.Mercancias.PesoNetoTotal == 0)
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Peso neto total incorrecto (PesoNetoTotal)");
                }
            }
            
            if (cartaPorte.Mercancias.Mercancia != null)
            {
                foreach (var mkt in cartaPorte.Mercancias.Mercancia)
                {
                    if (string.IsNullOrEmpty(mkt.BienesTransp))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Clave de mercancia no especificada (BienesTransp)");
                    }
                    if (mkt.Cantidad == 0)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Cantidad total de mercancias en cero incorrecta");
                    }
                    if (mkt.CantidadTransporta != null)
                    {
                        foreach (var ct in mkt.CantidadTransporta)
                        {
                            if (ct.Cantidad == 0)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Cantidad transportada de una mercancia en cero incorrecta");
                            }
                            if (ct.CvesTransporteSpecified)
                            {
                                if (!string.IsNullOrEmpty(ct.CvesTransporte))
                                {
                                    dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_CveTransporte.ToString(), "CvesTransporte", ct.CvesTransporte);
                                }
                            }
                            if (!string.IsNullOrEmpty(ct.IDDestino))
                            {
                                if (cartaPorte.Ubicaciones == null)
                                {
                                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "IDDestino de una mercancia no encontrado en ubicaciones");
                                }
                                else
                                {
                                    var rs = cartaPorte.Ubicaciones.Where(x => x.IDUbicacion == ct.IDDestino).Count();
                                    if (rs<=0)
                                    {
                                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "IDDestino (" + ct.IDDestino + ") de una mercancia no encontrado en ubicaciones");
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ct.IDOrigen))
                            {
                                if(cartaPorte.Ubicaciones==null)
                                {
                                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "IDOrigen de una mercancia no encontrado en ubicaciones");
                                }
                                else
                                {
                                    var rs = cartaPorte.Ubicaciones.Where(x => x.IDUbicacion == ct.IDOrigen).Count();
                                    if(rs<=0)
                                    {
                                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "IDOrigen (" + ct.IDOrigen + ") de una mercancia no encontrado en ubicaciones");
                                    }
                                }
                            }
                            
                        }
                    }
                    if (!string.IsNullOrEmpty(mkt.ClaveSTCC))
                    {
                        pattern = getPattern(dataPattern, "Mercancia.ClaveSTCC");
                        if (!string.IsNullOrEmpty(pattern))
                        {
                            Match me = Regex.Match(mkt.ClaveSTCC, pattern, RegexOptions.IgnoreCase);
                            if (!me.Success)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Clave STCC para una mercancia incorrecta (ClaveSTCC)");
                            }
                        }
                        else
                        {
                            Log.Error("Error de carga de patron para CartaPorteRepository Mercancia.ClaveSTCC");
                        }
                    }
                    if (string.IsNullOrEmpty(mkt.ClaveUnidad))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Clave de unidad para una mercancia no especificada (ClaveUnidad)");
                    }
                    else
                    {
                        dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ClaveUnidad.ToString(), "ClaveUnidad", mkt.ClaveUnidad);
                    }
                    if (mkt.CveMaterialPeligrosoSpecified)
                    {
                        if (string.IsNullOrEmpty(mkt.CveMaterialPeligroso))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "No se especifico la clave de material peligroso para una mercancia (CveMaterialPeligroso)");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_MaterialPeligroso.ToString(), "CveMaterialPeligroso", mkt.CveMaterialPeligroso);
                        }
                    }
                    if (string.IsNullOrEmpty(mkt.Descripcion))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Descripcion de mercancia requerida");
                    }
                    if (!string.IsNullOrEmpty(mkt.DescripEmbalaje))
                    {
                        if (mkt.DescripEmbalaje.Length > 100)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud de la descripcion del embalaje mayor que la permitida (DescripEmbalaje)");
                        }
                    }
                    if (mkt.DetalleMercancia != null)
                    {
                        if (mkt.DetalleMercancia.NumPiezasSpecified)
                        {
                            //opcional mkt.DetalleMercancia.NumPiezas
                            //if (mkt.DetalleMercancia.NumPiezas == 0) { }
                        }
                        if (mkt.DetalleMercancia.PesoBruto == 0)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Peso bruto en detalle de mercancia en cero invalido");
                        }
                        if (mkt.DetalleMercancia.PesoNeto == 0)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Peso neto en detalle de mercancia en cero invalido");
                        }
                        if (mkt.DetalleMercancia.PesoTara == 0)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Peso tara en detalle de mercancia en cero invalido");
                        }
                        if (string.IsNullOrEmpty(mkt.DetalleMercancia.UnidadPesoMerc))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Unidad de peso en el detalle de mercancia no especificada (UnidadPesoMerc)");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ClaveUnidadPeso.ToString(), "UnidadPesoMerc", mkt.DetalleMercancia.UnidadPesoMerc);
                        }
                    }
                    if (!string.IsNullOrEmpty(mkt.Dimensiones))
                    {
                        pattern = getPattern(dataPattern, "Mercancia.Dimensiones");
                        if (!string.IsNullOrEmpty(pattern))
                        {
                            Match me = Regex.Match(mkt.Dimensiones, pattern, RegexOptions.IgnoreCase);
                            if (!me.Success)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Especificacion de las dimensiones incorrecta para una mercancia");
                            }
                        }
                        else
                        {
                            Log.Error("Error de carga de patron para CartaPorteRepository Mercancia.Dimensiones");
                        }
                    }
                    if (mkt.EmbalajeSpecified)
                    {
                        if (string.IsNullOrEmpty(mkt.Embalaje))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Clave de embalaje no proporcionada para una mercancia");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_TipoEmbalaje.ToString(), "Embalaje", mkt.Embalaje);
                        }
                    }
                    if (mkt.FraccionArancelariaSpecified)
                    {
                        if (string.IsNullOrEmpty(mkt.FraccionArancelaria))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Fraccion arancelaria no especificada para una mercancia");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_FraccionArancelaria.ToString(), "FraccionArancelaria", mkt.FraccionArancelaria);
                        }
                    }
                    if (mkt.GuiasIdentificacion != null)
                    {
                        foreach (var gi in mkt.GuiasIdentificacion)
                        {
                            if (string.IsNullOrEmpty(gi.DescripGuiaIdentificacion))
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Descripcion del contenido de mercancia requerido (DescripGuiaIdentificacion)");
                            }
                            if (string.IsNullOrEmpty(gi.NumeroGuiaIdentificacion))
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Numero de guia identificacion no proporcionado para una mercancia");
                            }
                            if (gi.PesoGuiaIdentificacion == 0)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Peso requerido en guia identificacion");
                            }
                        }
                    }
                    if (mkt.MaterialPeligrosoSpecified)
                    {
                        if (!string.IsNullOrEmpty(mkt.MaterialPeligroso))
                        {
                            if (!(mkt.MaterialPeligroso.ToUpper() == "SÍ" ||
                                mkt.MaterialPeligroso.ToUpper() == "SI" ||
                                mkt.MaterialPeligroso.ToUpper() == "NO"))
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Valor incorrecto al indicar si una mercancia es peligrosa o no (MaterialPeligroso)");
                            }
                        }
                    }
                    if (mkt.MonedaSpecified)
                    {
                        if (string.IsNullOrEmpty(mkt.Moneda))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Moneda no especificada");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Moneda.ToString(), "Moneda", mkt.Moneda);
                        }
                    }
                    if (mkt.Pedimentos != null)
                    {
                        foreach (var pd in mkt.Pedimentos)
                        {
                            if (string.IsNullOrEmpty(pd.Pedimento))
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Pedimento no especificado para una mercancia");
                            }
                            else
                            {
                                pattern = getPattern(dataPattern, "Pedimentos.Pedimento");
                                if (!string.IsNullOrEmpty(pattern))
                                {
                                    Match me = Regex.Match(pd.Pedimento, pattern, RegexOptions.IgnoreCase);
                                    if (!me.Success)
                                    {
                                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Formato de pedimento incorrecto para una mercancia");
                                    }
                                }
                                else
                                {
                                    Log.Error("Error de carga de patron para CartaPorteRepository Pedimentos.Pedimento");
                                }
                            }
                        }
                    }
                    if (mkt.PesoEnKg == 0)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Peso en kg en cero para una mercancia no permitido");
                    }
                    if (!string.IsNullOrEmpty(mkt.Unidad))
                    {
                        if (mkt.Unidad.Length > 20)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud de la descripcion de la unidad mayor a la permitida");
                        }
                    }
                    if (!string.IsNullOrEmpty(mkt.UUIDComercioExt))
                    {
                        pattern = getPattern(dataPattern, "Mercancia.UUIDComercioExt");
                        if (!string.IsNullOrEmpty(pattern))
                        {
                            Match me = Regex.Match(mkt.UUIDComercioExt, pattern, RegexOptions.IgnoreCase);
                            if (!me.Success)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "UUIDComercioExt incorrecta para una mercancia");
                            }
                        }
                        else
                        {
                            Log.Error("Error de carga de patron para CartaPorteRepository Mercancia.UUIDComercioExt");
                        }
                    }
                    if (mkt.ValorMercanciaSpecified)
                    {
                        //opcional ValorMercancia
                        //if (mkt.ValorMercancia == 0) { }
                    }
                }
            }
            if (string.IsNullOrEmpty(cartaPorte.Mercancias.UnidadPeso))
            {
                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Unidad de peso no especificada (UnidadPeso)");
            }
            else
            {
                dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_ClaveUnidadPeso.ToString(), "UnidadPeso", cartaPorte.Mercancias.UnidadPeso);
            }

            return validationErrors;
        }

        private List<ValidationError> CartaPorteUbicaciones(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, CartaPorte cartaPorte, string sourceDocument, bool esNotaCredito)
        {
            if (cartaPorte.Ubicaciones != null)
            {
                int qtyOrigen, qtyDestino;
                qtyOrigen = qtyDestino = 0;

                foreach (var ub in cartaPorte.Ubicaciones)
                {
                    if (ub.DistanciaRecorridaSpecified)
                    {
                        if (ub.DistanciaRecorrida == 0)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Valor de la distancia recorrida incorrecto (DistanciaRecorrida)");
                        }
                    }
                    if (ub.Domicilio != null)
                    {
                        //opcional ub.Domicilio.Calle
                        //if (String.IsNullOrEmpty(ub.Domicilio.Calle)) { }
                        if (string.IsNullOrEmpty(ub.Domicilio.CodigoPostal))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Codigo postal del domicilio no especificado (CodigoPostal)");
                        }
                        //opcional ub.Domicilio.Colonia
                        //if (String.IsNullOrEmpty(ub.Domicilio.Colonia)) { }
                        //opcional ub.Domicilio.Localidad
                        //if (String.IsNullOrEmpty(ub.Domicilio.Localidad)) { }
                        //opcional ub.Domicilio.Municipio
                        //if (String.IsNullOrEmpty(ub.Domicilio.Municipio)) { }
                        //opcional ub.Domicilio.NumeroExterior
                        //if (String.IsNullOrEmpty(ub.Domicilio.NumeroExterior)) { }
                        //opcional ub.Domicilio.NumeroInterior
                        //if (String.IsNullOrEmpty(ub.Domicilio.NumeroInterior)) { }
                        if (string.IsNullOrEmpty(ub.Domicilio.Pais))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Pais del domicilio no especificada (Pais)");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Pais.ToString(), "Pais", ub.Domicilio.Pais);
                        }
                        //opcional ub.Domicilio.Referencia
                        //if (String.IsNullOrEmpty(ub.Domicilio.Referencia)) { }
                        if (string.IsNullOrEmpty(ub.Domicilio.Estado))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Estado del domicilio requerido");
                        }
                        else
                        {
                            if (ub.Domicilio.Estado.Length > 30)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud del Estado del domicilio mayor que la permitida (Estado)");
                            }
                        }
                    }
                    if (ub.FechaHoraSalidaLlegada == DateTime.MinValue)
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Valor incorrecto para el campo (FechaHoraSalidaLlegada)");
                    }
                    if (!string.IsNullOrEmpty(ub.IDUbicacion))
                    {
                        pattern = getPattern(dataPattern, "Ubicaciones.IDUbicacion");
                        if (!string.IsNullOrEmpty(pattern))
                        {
                            Match me = Regex.Match(ub.IDUbicacion, pattern, RegexOptions.IgnoreCase);
                            if (!me.Success)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "IDUbicacion incorrecto");
                            }
                        }
                        else
                        {
                            Log.Error("Error de carga de patron para CartaPorteRepository Ubicaciones.IDUbicacion");
                        }
                    }
                    if (ub.NavegacionTraficoSpecified)
                    {
                        if (string.IsNullOrEmpty(ub.NavegacionTrafico))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Campo (NavegacionTrafico) requerido");
                        }
                        else
                        {
                            if (!(ub.NavegacionTrafico.ToUpper() == "ALTURA" ||
                                ub.NavegacionTrafico.ToUpper() == "CABOTAJE"))
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Valor incorrecto para el tipo de puerto (NavegacionTrafico)");
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(ub.NombreEstacion))
                    {
                        dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Estaciones.ToString(), "NombreEstacion", ub.NombreEstacion);
                    }
                    if (!string.IsNullOrEmpty(ub.NombreRemitenteDestinatario))
                    {
                        if (ub.NombreRemitenteDestinatario.Length > 254)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Longitud de datos mayor a la permitida (NombreRemitenteDestinatario)");
                        }
                    }
                    if (ub.NumEstacionSpecified)
                    {
                        if (!string.IsNullOrEmpty(ub.NumEstacion))
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Estaciones.ToString(), "NumEstacion", ub.NumEstacion);
                        }
                    }
                    if (!string.IsNullOrEmpty(ub.NumRegIdTrib))
                    {
                        if (ub.NumRegIdTrib.Length < 6 || ub.NumRegIdTrib.Length > 40)
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tamaño del registro fiscal incorrecto (NumRegIdTrib)");
                        }
                    }
                    if (ub.ResidenciaFiscalSpecified)
                    {
                        if (!string.IsNullOrEmpty(ub.ResidenciaFiscal))
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Pais.ToString(), "ResidenciaFiscal", ub.ResidenciaFiscal);
                        }
                    }
                    if (string.IsNullOrEmpty(ub.RFCRemitenteDestinatario))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Remitente destinatario no especificado (RFCRemitenteDestinatario)");
                    }
                    if (ub.TipoEstacionSpecified)
                    {
                        if (string.IsNullOrEmpty(ub.TipoEstacion))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "campo (TipoEstacion) no especificado ");
                        }
                        else
                        {
                            dataSearch = itemSearch(dataSearch, ValidationCFDI.Tables.c_Estaciones.ToString(), "TipoEstacion", ub.TipoEstacion);
                        }
                    }
                    if (string.IsNullOrEmpty(ub.TipoUbicacion))
                    {
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Tipo de ubicación requerido (TipoUbicacion)");
                    }
                    else
                    {
                        if (!(ub.TipoUbicacion.ToUpper() == "ORIGEN" ||
                            ub.TipoUbicacion.ToUpper() == "DESTINO"))
                        {
                            validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Valor incorrecto para el tipo de ubicacion (TipoUbicacion)");
                        }
                        else
                        {
                            if (ub.TipoUbicacion.ToUpper() == "ORIGEN") qtyOrigen++;
                            if (ub.TipoUbicacion.ToUpper() == "DESTINO") qtyDestino++;
                        }
                    }
                }

                if (qtyOrigen == 0)
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "No se especifico el origen en las ubicaciones");
                }
                if (qtyDestino == 0)
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "No se especifico el destino en las ubicaciones");
                }
            }
            else
            {
                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Ubicaciones requeridas");
            }

            return validationErrors;
        }

        public async Task<List<ValidationError>> validateCartaPorte(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, CartaPorte cartaPorte, string sourceDocument, bool esNotaCredito)
        {
            dataSearch = new List<DataSearch>() { };
            dataPattern = new List<DataSearchRegExp>() { };

            try
            {
                dataPattern = await loadDataPattern();

                validationErrors = CartaPorteHdr(errorCatalog, validationErrors, cartaPorte, sourceDocument, esNotaCredito);

                if (cartaPorte.Mercancias != null)
                {
                    validationErrors = CartaPorteMercancia(errorCatalog, validationErrors, cartaPorte, sourceDocument, esNotaCredito);

                    validationErrors = CartaPorteAutoTransporte(errorCatalog, validationErrors, cartaPorte, sourceDocument, esNotaCredito);
                    validationErrors = CartaPorteTransporteAereo(errorCatalog, validationErrors, cartaPorte, sourceDocument, esNotaCredito);
                    validationErrors = CartaPorteTransporteFerroviario(errorCatalog, validationErrors, cartaPorte, sourceDocument, esNotaCredito);
                    validationErrors = CartaPorteTransporteMaritimo(errorCatalog, validationErrors, cartaPorte, sourceDocument, esNotaCredito);

                }
                else
                {
                    validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "No se proporcionó detalle de las mercancias");
                }

                validationErrors = CartaPorteUbicaciones(errorCatalog, validationErrors, cartaPorte, sourceDocument, esNotaCredito);

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

        public List<ValidationError> addErrorCP(bool esNotaCredito, IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, string clave, string claveNC, string documento, string descripcion)
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
                        validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Error al consultar base para datos de CartaPorte");
                    }
                    else
                    {
                        foreach (var it in dataSearch)
                        {
                            var r = cg.FirstOrDefault(x => x.tabla == it.tabla && x.campo == it.campo && x.dato == it.dato);
                            if (r == null)
                            {
                                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Campo (" + it.campo + ") con el valor (" + it.dato.ToString() + ") incorrecto segun catalogos del SAT");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                validationErrors = addErrorCP(esNotaCredito, errorCatalog, validationErrors, "65000", "65000", sourceDocument, "Error al consultar base para datos de CartaPorte");
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
