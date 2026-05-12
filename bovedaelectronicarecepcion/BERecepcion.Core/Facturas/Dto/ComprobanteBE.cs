using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace BERecepcion.Core.Facturas.Dto
{

    [XmlRoot(ElementName = "Emisor", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class Emisor
    {
        [XmlAttribute(AttributeName = "Rfc")]
        public string Rfc { get; set; }
        [XmlAttribute(AttributeName = "Nombre")]
        public string Nombre { get; set; }
        [XmlAttribute(AttributeName = "RegimenFiscal")]
        public int RegimenFiscal { get; set; }
    }

    [XmlRoot(ElementName = "Receptor", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class Receptor
    {
        [XmlAttribute(AttributeName = "Rfc")]
        public string Rfc { get; set; }
        [XmlAttribute(AttributeName = "Nombre")]
        public string Nombre { get; set; }
        [XmlAttribute(AttributeName = "UsoCFDI")]
        public string UsoCFDI { get; set; }
    }

    [XmlRoot(ElementName = "Traslado", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class Traslado
    {
        [XmlAttribute(AttributeName = "Base")]
        public double Base { get; set; }
        [XmlAttribute(AttributeName = "Impuesto")]
        public int Impuesto { get; set; }
        [XmlAttribute(AttributeName = "TipoFactor")]
        public string TipoFactor { get; set; }
        [XmlAttribute(AttributeName = "TasaOCuota")]
        public double TasaOCuota { get; set; }
        [XmlAttribute(AttributeName = "Importe")]
        public double Importe { get; set; }
    }

    [XmlRoot(ElementName = "Traslados", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class Traslados
    {
        [XmlElement(ElementName = "Traslado", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public Traslado Traslado { get; set; }
    }

    [XmlRoot(ElementName = "Impuestos", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class Impuestos
    {
        [XmlElement(ElementName = "Traslados", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public Traslados Traslados { get; set; }
        [XmlElement(ElementName = "Retenciones", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public Retenciones Retenciones { get; set; }
        [XmlAttribute(AttributeName = "TotalImpuestosTrasladados")]
        public double TotalImpuestosTrasladados { get; set; }
        [XmlAttribute(AttributeName = "TotalImpuestosRetenidos")]
        public double TotalImpuestosRetenidos { get; set; }
    }

    [XmlRoot(ElementName = "Concepto", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class Concepto
    {
        [XmlElement(ElementName = "Impuestos", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public Impuestos Impuestos { get; set; }
        [XmlAttribute(AttributeName = "ClaveProdServ")]
        public int ClaveProdServ { get; set; }
        [XmlAttribute(AttributeName = "NoIdentificacion")]
        public string NoIdentificacion { get; set; }
        [XmlAttribute(AttributeName = "Cantidad")]
        public double Cantidad { get; set; }
        [XmlAttribute(AttributeName = "ClaveUnidad")]
        public string ClaveUnidad { get; set; }
        [XmlAttribute(AttributeName = "Unidad")]
        public string Unidad { get; set; }
        [XmlAttribute(AttributeName = "Descripcion")]
        public string Descripcion { get; set; }
        [XmlAttribute(AttributeName = "ValorUnitario")]
        public double ValorUnitario { get; set; }
        [XmlAttribute(AttributeName = "Importe")]
        public double Importe { get; set; }
    }

    [XmlRoot(ElementName = "Conceptos", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class Conceptos
    {
        [XmlElement(ElementName = "Concepto", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public List<Concepto> Concepto { get; set; }
    }

    [XmlRoot(ElementName = "Retencion", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class Retencion
    {
        [XmlAttribute(AttributeName = "Base")]
        public double Base { get; set; }
        [XmlAttribute(AttributeName = "Impuesto")]
        public string Impuesto { get; set; }
        [XmlAttribute(AttributeName = "TipoFactor")]
        public string TipoFactor { get; set; }
        [XmlAttribute(AttributeName = "TasaOCuota")]
        public double TasaOCuota { get; set; }
        [XmlAttribute(AttributeName = "Importe")]
        public double Importe { get; set; }
    }

    [XmlRoot(ElementName = "Retenciones", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class Retenciones
    {
        [XmlElement(ElementName = "Retencion", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public Retencion Retencion { get; set; }
    }

    [XmlRoot(ElementName = "TimbreFiscalDigital", Namespace = "http://www.sat.gob.mx/TimbreFiscalDigital")]
    public class TimbreFiscalDigital
    {
        [XmlAttribute(AttributeName = "tfd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Tfd { get; set; }
        [XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string SchemaLocation { get; set; }
        [XmlAttribute(AttributeName = "SelloSAT")]
        public string SelloSAT { get; set; }
        [XmlAttribute(AttributeName = "NoCertificadoSAT")]
        public string NoCertificadoSAT { get; set; }
        [XmlAttribute(AttributeName = "SelloCFD")]
        public string SelloCFD { get; set; }
        [XmlAttribute(AttributeName = "RfcProvCertif")]
        public string RfcProvCertif { get; set; }
        [XmlAttribute(AttributeName = "FechaTimbrado")]
        public DateTime FechaTimbrado { get; set; }
        [XmlAttribute(AttributeName = "UUID")]
        public Guid UUID { get; set; }
        [XmlAttribute(AttributeName = "Version")]
        public string Version { get; set; }
    }

    [XmlRoot(ElementName = "Complemento", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class Complemento
    {
        [XmlElement(ElementName = "Pagos", Namespace = "http://www.sat.gob.mx/Pagos")]
        public Pagos Pagos { get; set; }

        [XmlElement(ElementName = "CartaPorte", Namespace = "http://www.sat.gob.mx/CartaPorte20")]
        public CartaPorte CartaPorte { get; set; }

        [XmlElement(ElementName = "TimbreFiscalDigital", Namespace = "http://www.sat.gob.mx/TimbreFiscalDigital")]
        public TimbreFiscalDigital TimbreFiscalDigital { get; set; }
    }

    [XmlRoot(ElementName = "Addenda_Pemex", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
    public class Addenda_Pemex
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

    [XmlRoot(ElementName = "Addenda", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class Addenda
    {
        [XmlElement(ElementName = "Addenda_Pemex", Namespace = "http://pemex.com/facturaelectronica/addenda/v2")]
        public Addenda_Pemex Addenda_Pemex { get; set; }
    }

    [XmlRoot(ElementName = "Comprobante", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class ComprobanteBE
    {
        [XmlElement(ElementName = "CfdiRelacionados", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public CfdiRelacionados CfdiRelacionados { get; set; }
        [XmlElement(ElementName = "Emisor", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public Emisor Emisor { get; set; }
        [XmlElement(ElementName = "Receptor", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public Receptor Receptor { get; set; }
        [XmlElement(ElementName = "Conceptos", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public Conceptos Conceptos { get; set; }
        [XmlElement(ElementName = "Impuestos", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public Impuestos Impuestos { get; set; }
        [XmlElement(ElementName = "Complemento", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public Complemento Complemento { get; set; }
        [XmlElement(ElementName = "Addenda", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public Addenda Addenda { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "cfdi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Cfdi { get; set; }
        [XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string SchemaLocation { get; set; }
        [XmlAttribute(AttributeName = "Version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "Serie")]
        public string Serie { get; set; }
        [XmlAttribute(AttributeName = "Folio")]
        public string Folio { get; set; }
        [XmlAttribute(AttributeName = "Fecha")]
        public string Fecha { get; set; }
        [XmlAttribute(AttributeName = "Sello")]
        public string Sello { get; set; }
        [XmlAttribute(AttributeName = "FormaPago")]
        public string FormaPago { get; set; }
        [XmlAttribute(AttributeName = "NoCertificado")]
        public string NoCertificado { get; set; }
        [XmlAttribute(AttributeName = "Certificado")]
        public string Certificado { get; set; }
        [XmlAttribute(AttributeName = "CondicionesDePago")]
        public string CondicionesDePago { get; set; }
        [XmlAttribute(AttributeName = "SubTotal")]
        public double SubTotal { get; set; }
        [XmlAttribute(AttributeName = "Moneda")]
        public string Moneda { get; set; }
        [XmlAttribute(AttributeName = "Total")]
        public double Total { get; set; }
        [XmlAttribute(AttributeName = "TipoDeComprobante")]
        public string TipoDeComprobante { get; set; }
        [XmlAttribute(AttributeName = "MetodoPago")]
        public string MetodoPago { get; set; }
        [XmlAttribute(AttributeName = "LugarExpedicion")]
        public string LugarExpedicion { get; set; }
        public virtual string FileName { get; set; }
    }

    // para Complemento de Pagos

    [XmlRoot(ElementName = "DoctoRelacionado", Namespace = "http://www.sat.gob.mx/Pagos")]
    public class DoctoRelacionado
    {
        [XmlAttribute(AttributeName = "IdDocumento")]
        public string IdDocumento { get; set; }
        [XmlAttribute(AttributeName = "Folio")]
        public string Folio { get; set; }
        [XmlAttribute(AttributeName = "MonedaDR")]
        public string MonedaDR { get; set; }
        [XmlAttribute(AttributeName = "MetodoDePagoDR")]
        public string MetodoDePagoDR { get; set; }
        [XmlAttribute(AttributeName = "NumParcialidad")]
        public string NumParcialidad { get; set; }
        [XmlAttribute(AttributeName = "ImpSaldoAnt")]
        public string ImpSaldoAnt { get; set; }
        [XmlAttribute(AttributeName = "ImpPagado")]
        public string ImpPagado { get; set; }
        [XmlAttribute(AttributeName = "ImpSaldoInsoluto")]
        public string ImpSaldoInsoluto { get; set; }
        public virtual bool ExisteEnBD { get; set; }
    }

    [XmlRoot(ElementName = "Pago", Namespace = "http://www.sat.gob.mx/Pagos")]
    public class Pago
    {
        [XmlElement(ElementName = "DoctoRelacionado", Namespace = "http://www.sat.gob.mx/Pagos")]
        public List<DoctoRelacionado> DoctoRelacionado { get; set; }
        [XmlAttribute(AttributeName = "FechaPago")]
        public string FechaPago { get; set; }
        [XmlAttribute(AttributeName = "FormaDePagoP")]
        public string FormaDePagoP { get; set; }
        [XmlAttribute(AttributeName = "MonedaP")]
        public string MonedaP { get; set; }
        [XmlAttribute(AttributeName = "Monto")]
        public string Monto { get; set; }
        [XmlAttribute(AttributeName = "NumOperacion")]
        public string NumOperacion { get; set; }
        [XmlAttribute(AttributeName = "RfcEmisorCtaOrd")]
        public string RfcEmisorCtaOrd { get; set; }
        [XmlAttribute(AttributeName = "NomBancoOrdExt")]
        public string NomBancoOrdExt { get; set; }
        [XmlAttribute(AttributeName = "CtaOrdenante")]
        public string CtaOrdenante { get; set; }
        [XmlAttribute(AttributeName = "RfcEmisorCtaBen")]
        public string RfcEmisorCtaBen { get; set; }
        [XmlAttribute(AttributeName = "CtaBeneficiario")]
        public string CtaBeneficiario { get; set; }
    }

    [XmlRoot(ElementName = "Pagos", Namespace = "http://www.sat.gob.mx/Pagos")]
    public class Pagos
    {
        [XmlElement(ElementName = "Pago", Namespace = "http://www.sat.gob.mx/Pagos")]
        public Pago Pago { get; set; }
        [XmlAttribute(AttributeName = "Version")]
        public string Version { get; set; }
    }

    [XmlRoot(ElementName = "CfdiRelacionado", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class CfdiRelacionado
    {
        [XmlAttribute(AttributeName = "UUID")]
        public string UUID { get; set; }
    }

    [XmlRoot(ElementName = "CfdiRelacionados", Namespace = "http://www.sat.gob.mx/cfd/3")]
    public class CfdiRelacionados
    {
        [XmlElement(ElementName = "CfdiRelacionado", Namespace = "http://www.sat.gob.mx/cfd/3")]
        public CfdiRelacionado CfdiRelacionado { get; set; }
        [XmlAttribute(AttributeName = "TipoRelacion")]
        public string TipoRelacion { get; set; }
    }
}
