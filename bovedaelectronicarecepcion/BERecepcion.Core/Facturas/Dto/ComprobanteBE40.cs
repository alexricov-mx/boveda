using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BERecepcion.Core.Facturas.Dto
{
    [XmlRoot(ElementName = "Emisor", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteEmisor
    {

        [XmlAttribute(AttributeName = "Rfc")]
        public string Rfc { get; set; }

        [XmlAttribute(AttributeName = "Nombre")]
        public string Nombre { get; set; }

        [XmlAttribute(AttributeName = "RegimenFiscal")]
        public string RegimenFiscal { get; set; }

        [XmlAttribute(AttributeName = "FacAtrAdquirente")]
        public string FacAtrAdquirente { get; set; }
    }

    [XmlRoot(ElementName = "Receptor", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteReceptor
    {
        [XmlAttribute(AttributeName = "Rfc")]
        public string Rfc { get; set; }
        [XmlAttribute(AttributeName = "Nombre")]
        public string Nombre { get; set; }
        [XmlAttribute(AttributeName = "DomicilioFiscalReceptor")]
        public string DomicilioFiscalReceptor { get; set; }
        [XmlAttribute(AttributeName = "ResidenciaFiscal")]
        public string ResidenciaFiscal { get; set; }
        [XmlAttribute(AttributeName = "ResidenciaFiscalSpecified")]
        public bool ResidenciaFiscalSpecified { get; set; }
        [XmlAttribute(AttributeName = "NumRegIdTrib")]
        public string NumRegIdTrib { get; set; }
        [XmlAttribute(AttributeName = "RegimenFiscalReceptor")]
        public string RegimenFiscalReceptor { get; set; }
        [XmlAttribute(AttributeName = "UsoCFDI")]
        public string UsoCFDI { get; set; }
    }

    [XmlRoot(ElementName = "Traslado", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteConceptoImpuestosTraslado
    {
        [XmlAttribute(AttributeName = "Base")]
        public decimal Base { get; set; }
        [XmlAttribute(AttributeName = "Impuesto")]
        public string Impuesto { get; set; }
        [XmlAttribute(AttributeName = "TipoFactor")]
        public string TipoFactor { get; set; }
        [XmlAttribute(AttributeName = "TasaOCuota")]
        public decimal TasaOCuota { get; set; }
        [XmlAttribute(AttributeName = "TasaOCuotaSpecified")]
        public bool TasaOCuotaSpecified { get; set; }
        [XmlAttribute(AttributeName = "Importe")]
        public decimal Importe { get; set; }
        [XmlAttribute(AttributeName = "ImporteSpecified")]
        public bool ImporteSpecified { get; set; }
    }

    [XmlRoot(ElementName = "Retencion", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteConceptoImpuestosRetencion
    {
        [XmlAttribute(AttributeName = "Base")]
        public decimal Base { get; set; }
        [XmlAttribute(AttributeName = "Impuesto")]
        public string Impuesto { get; set; }
        [XmlAttribute(AttributeName = "TipoFactor")]
        public string TipoFactor { get; set; }
        [XmlAttribute(AttributeName = "TasaOCuota")]
        public decimal TasaOCuota { get; set; }
        [XmlAttribute(AttributeName = "Importe")]
        public decimal Importe { get; set; }
    }

    [XmlRoot(ElementName = "InformacionGlobal", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteInformacionGlobal
    {
        [XmlAttribute(AttributeName = "Periodicidad")]
        public string Periodicidad { get; set; }
        [XmlAttribute(AttributeName = "Meses")]
        public string Meses { get; set; }
        [XmlAttribute(AttributeName = "Año")]
        public short Año { get; set; }
    }

    [XmlRoot(ElementName = "CfdiRelacionados", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteCfdiRelacionados
    {
        [XmlArray("CfdiRelacionados")]
        [XmlArrayItem("CfdiRelacionado")]
        public ComprobanteCfdiRelacionadosCfdiRelacionado[] CfdiRelacionados { get; set; }
        [XmlAttribute(AttributeName = "TipoRelacion")]
        public string TipoRelacion { get; set; }
    }

    [XmlRoot(ElementName = "CfdiRelacionado", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteCfdiRelacionadosCfdiRelacionado
    {
        [XmlAttribute(AttributeName = "UUID")]
        public string UUID { get; set; }
    }

    [XmlRoot(ElementName = "Concepto", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public partial class ComprobanteConcepto
    {
        [XmlElement(ElementName = "Impuestos")]
        public ComprobanteConceptoImpuestos Impuestos { get; set; }
        [XmlElement(ElementName = "ACuentaTerceros")]
        public ComprobanteConceptoACuentaTerceros ACuentaTerceros { get; set; }
        [XmlArray("InformacionAduanera")]
        [XmlArrayItem("InformacionAduanera")]
        public ComprobanteConceptoInformacionAduanera[] InformacionAduanera { get; set; }
        [XmlElement(ElementName = "CuentaPredial")]
        public ComprobanteConceptoCuentaPredial[] CuentaPredial { get; set; }
        [XmlElement(ElementName = "ComplementoConcepto")]
        public ComprobanteConceptoComplementoConcepto ComplementoConcepto { get; set; }
        [XmlArray("Parte")]
        [XmlArrayItem("Parte")]
        public ComprobanteConceptoParte[] Parte { get; set; }
        [XmlAttribute(AttributeName = "ClaveProdServ")]
        public string ClaveProdServ { get; set; }
        [XmlAttribute(AttributeName = "NoIdentificacion")]
        public string NoIdentificacion { get; set; }
        [XmlAttribute(AttributeName = "Cantidad")]
        public decimal Cantidad { get; set; }
        [XmlAttribute(AttributeName = "ClaveUnidad")]
        public string ClaveUnidad { get; set; }
        [XmlAttribute(AttributeName = "Unidad")]
        public string Unidad { get; set; }
        [XmlAttribute(AttributeName = "Descripcion")]
        public string Descripcion { get; set; }
        [XmlAttribute(AttributeName = "ValorUnitario")]
        public decimal ValorUnitario { get; set; }
        [XmlAttribute(AttributeName = "Importe")]
        public decimal Importe { get; set; }
        [XmlAttribute(AttributeName = "Descuento")]
        public decimal Descuento { get; set; }
        [XmlAttribute(AttributeName = "DescuentoSpecified")]
        public bool DescuentoSpecified { get; set; }
        [XmlAttribute(AttributeName = "ObjetoImp")]
        public string ObjetoImp { get; set; }
    }

    [XmlRoot(ElementName = "Impuestos", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteConceptoImpuestos
    {
        [XmlArray("Traslados")]
        [XmlArrayItem("Traslado")]
        public ComprobanteConceptoImpuestosTraslado[] Traslados { get; set; }
        [XmlArray("Retenciones")]
        [XmlArrayItem("Retencion")]
        public ComprobanteConceptoImpuestosRetencion[] Retenciones { get; set; }
    }

    [XmlRoot(ElementName = "ACuentaTerceros", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteConceptoACuentaTerceros
    {
        [XmlAttribute(AttributeName = "RfcACuentaTerceros")]
        public string RfcACuentaTerceros { get; set; }
        [XmlAttribute(AttributeName = "NombreACuentaTerceros")]
        public string NombreACuentaTerceros { get; set; }
        [XmlAttribute(AttributeName = "RegimenFiscalACuentaTerceros")]
        public string RegimenFiscalACuentaTerceros { get; set; }
        [XmlAttribute(AttributeName = "DomicilioFiscalACuentaTerceros")]
        public string DomicilioFiscalACuentaTerceros { get; set; }
    }

    [XmlRoot(ElementName = "InformacionAduanera", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteConceptoInformacionAduanera
    {
        [XmlAttribute(AttributeName = "NumeroPedimento")]
        public string NumeroPedimento { get; set; }
    }

    [XmlRoot(ElementName = "CuentaPredial", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteConceptoCuentaPredial
    {
        [XmlAttribute(AttributeName = "Numero")]
        public string Numero { get; set; }
    }

    [XmlRoot(ElementName = "ComplementoConcepto", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteConceptoComplementoConcepto
    {
        [XmlElement(ElementName = "Any")]
        public System.Xml.XmlElement[] Any { get; set; }
    }

    [XmlRoot(ElementName = "Parte", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteConceptoParte
    {
        [XmlArray("InformacionAduanera")]
        [XmlArrayItem("InformacionAduanera")]
        public ComprobanteConceptoParteInformacionAduanera[] InformacionAduanera { get; set; }
        [XmlAttribute(AttributeName = "ClaveProdServ")]
        public string ClaveProdServ { get; set; }
        [XmlAttribute(AttributeName = "NoIdentificacion")]
        public string NoIdentificacion { get; set; }
        [XmlAttribute(AttributeName = "Cantidad")]
        public decimal Cantidad { get; set; }
        [XmlAttribute(AttributeName = "Unidad")]
        public string Unidad { get; set; }
        [XmlAttribute(AttributeName = "Descripcion")]
        public string Descripcion { get; set; }
        [XmlAttribute(AttributeName = "ValorUnitario")]
        public decimal ValorUnitario { get; set; }
        [XmlAttribute(AttributeName = "ValorUnitarioSpecified")]
        public bool ValorUnitarioSpecified { get; set; }
        [XmlAttribute(AttributeName = "Importe")]
        public decimal Importe { get; set; }
        [XmlAttribute(AttributeName = "ImporteSpecified")]
        public bool ImporteSpecified { get; set; }
    }

    [XmlRoot(ElementName = "InformacionAduanera", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteConceptoParteInformacionAduanera
    {
        [XmlAttribute(AttributeName = "NumeroPedimento")]
        public string NumeroPedimento { get; set; }
    }

    [XmlRoot(ElementName = "Impuestos", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteImpuestos
    {
        [XmlArray("Retenciones")]
        [XmlArrayItem("Retencion")]
        public ComprobanteImpuestosRetencion[] Retenciones { get; set; }
        [XmlArray("Traslados")]
        [XmlArrayItem("Traslado")]
        public ComprobanteImpuestosTraslado[] Traslados { get; set; }
        [XmlAttribute(AttributeName = "TotalImpuestosRetenidos")]
        public decimal TotalImpuestosRetenidos { get; set; }
        [XmlAttribute(AttributeName = "TotalImpuestosRetenidosSpecified")]
        public bool TotalImpuestosRetenidosSpecified { get; set; }
        [XmlAttribute(AttributeName = "TotalImpuestosTrasladados")]
        public decimal TotalImpuestosTrasladados { get; set; }
        [XmlAttribute(AttributeName = "TotalImpuestosTrasladadosSpecified")]
        public bool TotalImpuestosTrasladadosSpecified { get; set; }
    }

    [XmlRoot(ElementName = "Retencion", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteImpuestosRetencion
    {
        [XmlAttribute(AttributeName = "Impuesto")]
        public string Impuesto { get; set; }
        [XmlAttribute(AttributeName = "Importe")]
        public decimal Importe { get; set; }
    }

    [XmlRoot(ElementName = "Traslado", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteImpuestosTraslado
    {
        [XmlAttribute(AttributeName = "Base")]
        public decimal Base { get; set; }
        [XmlAttribute(AttributeName = "Impuesto")]
        public string Impuesto { get; set; }
        [XmlAttribute(AttributeName = "TipoFactor")]
        public string TipoFactor { get; set; }
        [XmlAttribute(AttributeName = "TasaOCuota")]
        public decimal TasaOCuota { get; set; }
        [XmlAttribute(AttributeName = "TasaOCuotaSpecified")]
        public bool TasaOCuotaSpecified { get; set; }
        [XmlAttribute(AttributeName = "Importe")]
        public decimal Importe { get; set; }
        [XmlAttribute(AttributeName = "ImporteSpecified")]
        public bool ImporteSpecified { get; set; }
    }

    [XmlRoot(ElementName = "Complemento", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteComplemento
    {
        [XmlElement(ElementName = "Pagos", Namespace = "http://www.sat.gob.mx/Pagos")]
        public Pagos Pagos { get; set; }

        [XmlElement(ElementName = "CartaPorte", Namespace = "http://www.sat.gob.mx/CartaPorte20")]
        public CartaPorte CartaPorte { get; set; }

        [XmlElement(ElementName = "TimbreFiscalDigital", Namespace = "http://www.sat.gob.mx/TimbreFiscalDigital")]
        public TimbreFiscalDigital TimbreFiscalDigital { get; set; }
    }

    [XmlRoot(ElementName = "Addenda_Pemex", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
    public class AddendaAddenda_Pemex
    {
        [XmlElement(ElementName = "CONTRATO", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string CONTRATO { get; set; }
        [XmlElement(ElementName = "O_SURTIMIENTO", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string O_SURTIMIENTO { get; set; }
        [XmlElement(ElementName = "N_ESTIMACION", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string N_ESTIMACION { get; set; }
        [XmlElement(ElementName = "P_ESTIMACION", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string P_ESTIMACION { get; set; }
        [XmlElement(ElementName = "N_ACREEDOR", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string N_ACREEDOR { get; set; }
        [XmlElement(ElementName = "C_GESTOR", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string C_GESTOR { get; set; }
        [XmlElement(ElementName = "DOSALMILLAR", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string DOSALMILLAR { get; set; }
        [XmlElement(ElementName = "FINIQUITO", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string FINIQUITO { get; set; }
        [XmlElement(ElementName = "POSICIONAP", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string POSICIONAP { get; set; }
        [XmlElement(ElementName = "AUTORIZA", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string AUTORIZA { get; set; }
        [XmlElement(ElementName = "EJERCICIO", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string EJERCICIO { get; set; }
        [XmlElement(ElementName = "ENTRADA", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string ENTRADA { get; set; }
        [XmlElement(ElementName = "CEJECUTOR", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string CEJECUTOR { get; set; }
        [XmlElement(ElementName = "RECEPSAP", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string RECEPSAP { get; set; }
        [XmlElement(ElementName = "PLAZO", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string PLAZO { get; set; }
        [XmlElement(ElementName = "RFCPROVEEDOR", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string RFCPROVEEDOR { get; set; }
        [XmlElement(ElementName = "REMESA", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string REMESA { get; set; }
        [XmlElement(ElementName = "NREMISION", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string NREMISION { get; set; }
        [XmlElement(ElementName = "VUREGION", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string VUREGION { get; set; }
        [XmlElement(ElementName = "FICHAE", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string FICHAE { get; set; }
        [XmlElement(ElementName = "FICHAF", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string FICHAF { get; set; }
        [XmlElement(ElementName = "MONEDA", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string MONEDA { get; set; }
        [XmlElement(ElementName = "FONDO", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string FONDO { get; set; }
        [XmlElement(ElementName = "POSICIONF", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string POSICIONF { get; set; }
        [XmlElement(ElementName = "OCOMERCIAL", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string OCOMERCIAL { get; set; }
        [XmlElement(ElementName = "SERVICIOG", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string SERVICIOG { get; set; }
        [XmlElement(ElementName = "SERVICIOA", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string SERVICIOA { get; set; }
        [XmlElement(ElementName = "CORREOPMI", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string CORREOPMI { get; set; }
        [XmlElement(ElementName = "CLAVE_TRANSP", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string CLAVE_TRANSP { get; set; }
        [XmlElement(ElementName = "A_RELACION", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string A_RELACION { get; set; }
        [XmlElement(ElementName = "ID_ANALITICO", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string ID_ANALITICO { get; set; }
        [XmlElement(ElementName = "TIPO_PRODUCTO", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string TIPO_PRODUCTO { get; set; }
        [XmlElement(ElementName = "CEDULA", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string CEDULA { get; set; }
        [XmlElement(ElementName = "CONTRATO_SIIC", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string CONTRATO_SIIC { get; set; }
        [XmlElement(ElementName = "ANALITICO", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public string ANALITICO { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "pm", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Pm { get; set; }
        [XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string SchemaLocation { get; set; }
    }

    [XmlRoot(ElementName = "Addenda", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteAddenda
    {
        [XmlElement(ElementName = "Addenda_Pemex", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public AddendaAddenda_Pemex Addenda_Pemex { get; set; }
    }

    [XmlRoot(ElementName = "Comprobante", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteBE40
    {
        [XmlElement(ElementName = "InformacionGlobal")]
        public ComprobanteInformacionGlobal InformacionGlobal { get; set; }
        [XmlArray("CfdiRelacionados")]
        [XmlArrayItem("CfdiRelacionado")]
        public ComprobanteCfdiRelacionadosCfdiRelacionado[] CfdiRelacionados { get; set; }
        [XmlElement(ElementName = "Emisor")]
        public ComprobanteEmisor Emisor { get; set; }
        [XmlElement(ElementName = "Receptor")]
        public ComprobanteReceptor Receptor { get; set; }
        [XmlArray("Conceptos")]
        [XmlArrayItem("Concepto")]
        public ComprobanteConcepto[] Conceptos { get; set; }
        [XmlElement(ElementName = "Impuestos")]
        public ComprobanteImpuestos Impuestos { get; set; }
        [XmlElement(ElementName = "Complemento")]
        public ComprobanteComplemento Complemento { get; set; }
        [XmlElement(ElementName = "Addenda")]
        public ComprobanteAddenda Addenda { get; set; }
        [XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string SchemaLocation { get; set; }
        [XmlAttribute(AttributeName = "Version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "Serie")]
        public string Serie { get; set; }
        [XmlAttribute(AttributeName = "Folio")]
        public string Folio { get; set; }
        [XmlAttribute(AttributeName = "Fecha")]
        public System.DateTime Fecha { get; set; }
        [XmlAttribute(AttributeName = "Sello")]
        public string Sello { get; set; }
        [XmlAttribute(AttributeName = "FormaPago")]
        public string FormaPago { get; set; }
        [XmlAttribute(AttributeName = "FormaPagoSpecified")]
        public bool FormaPagoSpecified { get; set; }
        [XmlAttribute(AttributeName = "NoCertificado")]
        public string NoCertificado { get; set; }
        [XmlAttribute(AttributeName = "Certificado")]
        public string Certificado { get; set; }
        [XmlAttribute(AttributeName = "CondicionesDePago")]
        public string CondicionesDePago { get; set; }
        [XmlAttribute(AttributeName = "SubTotal")]
        public decimal SubTotal { get; set; }
        [XmlAttribute(AttributeName = "Descuento")]
        public decimal Descuento { get; set; }
        [XmlAttribute(AttributeName = "DescuentoSpecified")]
        public bool DescuentoSpecified { get; set; }
        [XmlAttribute(AttributeName = "Moneda")]
        public string Moneda { get; set; }
        [XmlAttribute(AttributeName = "TipoCambio")]
        public decimal TipoCambio { get; set; }
        [XmlAttribute(AttributeName = "TipoCambioSpecified")]
        public bool TipoCambioSpecified { get; set; }
        [XmlAttribute(AttributeName = "Total")]
        public decimal Total { get; set; }
        [XmlAttribute(AttributeName = "TipoDeComprobante")]
        public string TipoDeComprobante { get; set; }
        [XmlAttribute(AttributeName = "Exportacion")]
        public string Exportacion { get; set; }
        [XmlAttribute(AttributeName = "MetodoPago")]
        public string MetodoPago { get; set; }
        [XmlAttribute(AttributeName = "MetodoPagoSpecified")]
        public bool MetodoPagoSpecified { get; set; }
        [XmlAttribute(AttributeName = "LugarExpedicion")]
        public string LugarExpedicion { get; set; }
        [XmlAttribute(AttributeName = "Confirmacion")]
        public string Confirmacion { get; set; }
    }
}