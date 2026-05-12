using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class CopadeDto
    {
        public Guid CopadeID { get; set; }
        public Guid OrganismID { get; set; }
        public string? Contract { get; set; }
        public string? SapOrder { get; set; }
        public string? Reception { get; set; }
        public string? Exercise { get; set; }
        public string? DocumentType { get; set; }
        public string? Center { get; set; }
        public DateTime? FechaEmision { get; set; }
        public string? Ficha { get; set; }
        public string? Agree { get; set; }
        public string? Authorizes { get; set; }
        public string? Creditor { get; set; }
        public string? CreditorBanking { get; set; }
        public string? CreditorNumber { get; set; }
        public string? CreditorRfc { get; set; }
        public string? Rfc { get; set; }
        public string? Society { get; set; }
        public string? CertificateNumber { get; set; }
        public string? ChangeType { get; set; }
        public string? Comments { get; set; }
        public string? Currency { get; set; }
        public string? Amount { get; set; }
        public string? EsTRI { get; set; }
        public string? Historical { get; set; }
        public string? Receipt { get; set; }
        public string? InvoiceStatus { get; set; }
        public string? Iva { get; set; }
        public string? PayEquity { get; set; }
        public string? PointAmount { get; set; }
        public DateTime? ReceptionDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Subtotal { get; set; }
        public string? Total { get; set; }
        public string? NoCad { get; set; }
        public string? Assignment { get; set; }
        public string? Desc { get; set; }
        public string? Esteem { get; set; }
        public string? Macroproject { get; set; }
        public string? RefPayment { get; set; }
        public string? Trust { get; set; }
        public string? AcceptDocument { get; set; }
        public string? ApplicablePunishment { get; set; }
        public DateTime? DeliveryLimitDate { get; set; }
        public DateTime? DeliveryRealDate { get; set; }
        public string? DiferenceDays { get; set; }
        public string? Division { get; set; }
        public string? EntryForm { get; set; }
        public string? EstimatedNumber { get; set; }
        public string? EstimationPeriod { get; set; }
        public string? Pospre { get; set; }
        public string? TaxSubtotal { get; set; }
        public string? TaxTotal { get; set; }
        public string? TaxTotalRetained { get; set; }
        public string? TaxTotalTransferred { get; set; }
        public string? TaxTotalLocalRetained { get; set; }
        public string? TaxTotalLocalTransferred { get; set; }
        public DateTime? ReleaseDateA { get; set; }
        public string? Functionary1Ficha { get; set; }
        public string? Functionary1Email { get; set; }
        public DateTime? Functionary1EmailSendDate { get; set; }
        public string? Functionary1EmailReason { get; set; }
        public DateTime? Functionary1SignDate { get; set; }
        public string? Functionary2Ficha { get; set; }
        public string? Functionary2Email { get; set; }
        public DateTime? Functionary2EmailSendDate { get; set; }
        public string? Functionary2EmailReason { get; set; }
        public DateTime? Functionary2SignDate { get; set; }
        public DateTime? FunctionaryNotifyPemexDate { get; set; }
        public string? ProviderEmail { get; set; }
        public DateTime? ProviderEmailSendDate { get; set; }
        public string? ProviderEmailReason { get; set; }
        public DateTime? ProviderNotifyPemexDate { get; set; }
        public bool? IsFullSigned { get; set; }
        public bool? IsCancel { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }
        public string? Detalle { get; set; }
        public string? DetalleAvion { get; set; }
        public string? DetalleObra { get; set; }
        public string? DetallePep { get; set; }
        public string? Impuestos { get; set; }
        public string? Complemento { get; set; }
        public string? PreFactura { get; set; }
        public string? NotasCredito { get; set; }
        public string? Addenda { get; set; }
        public string? PreFacturaXML { get; set; }
        public string? NotaCreditoXML { get; set; }
        public virtual string? clave { get; set; }
        public virtual IEnumerable<CopadeDetalle> vDetalle { get; set; }
        public virtual IEnumerable<CopadeDetalle> vDetalleAvion { get; set; }
        public virtual CopadeDetalleObra vDetalleObra { get; set; }
        public virtual IEnumerable<CopadeDetallePep> vDetallePep { get; set; }
        public virtual CopadeImpuestos vImpuestos { get; set; }
        public virtual CopadeComplemento vComplemento { get; set; }
        public virtual CopadePreFactura vPreFactura { get; set; }
        public virtual CopadeNotasCredito vNotasCredito { get; set; }
        public virtual CopadeAddendaPemex vAddenda { get; set; }
        public virtual string LogoB64 { get; set; }
        public virtual ComprobanteBE Comprobante { get; set; }
        public virtual IEnumerable<InvoiceDto> Factura { get; set; }
        public virtual IEnumerable<CopadeDto> Copade { get; set; }
        public virtual IEnumerable<InvoiceNotaCreditoDto> NotaCred { get; set; }
        public virtual IEnumerable<CopadeDto> Comprobantes { get; set; }
        public virtual Guid PagosID { get; set; }
        public virtual Guid PagoUUID { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string TipoDoc { get; set; }
        public virtual bool unicoFirmante { get; set; }
        public virtual string signer1 { get; set; }
        public virtual string fichaSigner1 { get; set; }
        public virtual string alternate1 { get; set; }
        public virtual string fichaAlternate1 { get; set; }
        public virtual string signer2 { get; set; }
        public virtual string fichaSigner2 { get; set; }
        public virtual string alternate2 { get; set; }
        public virtual string fichaAlternate2 { get; set; }

        public virtual string FunctionaryCancelName { get; set; }
        public virtual string Functionary1SignerName { get; set; }
        public virtual string Functionary2SignerName { get; set; }
        public virtual IEnumerable<DateHelperModel> Seguimiento { get; set; }
    }
    public class CopadeDetalle
    {
        public string Position { get; set; }
        public string Concept { get; set; }
        public string Perception { get; set; }
        public string Deduction { get; set; }
        public string Iva { get; set; }
    }

    public class CopadeDetallePep
    {
        public string ReceptionDate { get; set; }
        public string AcceptanceDate { get; set; }
        public string ChangeType { get; set; }
        public string SapOrder { get; set; }
        public string Amount { get; set; }
    }

    public class CopadeDetalleObra
    {
        public string Tipo { get; set; }
        public IEnumerable<CopadeDetalle> Detalle { get; set; }
    }
    public class CopadeImpuestos
    {
        public CopadeImpuestosRetenciones Retenciones { get; set; }
        public CopadeImpuestosTraslados Traslados { get; set; }
    }

    public class CopadeImpuestosRetenciones
    {
        public IEnumerable<CopadeImpuestosRetencionesRetencion> Retencion { get; set; }
    }

    public class CopadeImpuestosTraslados
    {
        public IEnumerable<CopadeImpuestosTrasladosTraslado> Traslado { get; set; }
    }

    public class CopadeImpuestosRetencionesRetencion
    {
        public string importe { get; set; }
        public string impuesto { get; set; }
    }

    public class CopadeImpuestosTrasladosTraslado
    {
        public string importe { get; set; }
        public string impuesto { get; set; }
        public string tasa { get; set; }
    }

    public class CopadeComplemento
    {
        public CopadeComplementoImpuestosLocales impuestosLocales { get; set; }
    }

    public class CopadeComplementoImpuestosLocales
    {
        public CopadeComplementoImpuestosLocalesRetencionesLocales RetencionLocales { get; set; }
        public string Total_de_retenciones { get; set; }
        public string Total_de_traslados { get; set; }
        public CopadeComplementoImpuestosLocalesTrasladosLocalesTrasladoLocal TrasladosLocales { get; set; }
    }

    public class CopadeComplementoImpuestosLocalesRetencionesLocales
    {
        public IEnumerable<CopadeComplementoImpuestosLocalesRetencionesLocalesRetencionLocal> RetencionLocal { get; set; }

    }

    public class CopadeComplementoImpuestosLocalesRetencionesLocalesRetencionLocal
    {
        public string importe { get; set; }
        public string imp_local_retenido { get; set; }
        public string tasa_de_retencion { get; set; }
    }

    public class CopadeComplementoImpuestosLocalesTrasladosLocales
    {
        public IEnumerable<CopadeComplementoImpuestosLocalesTrasladosLocalesTrasladoLocal> TrasladoLocal { get; set; }
    }

    public class CopadeComplementoImpuestosLocalesTrasladosLocalesTrasladoLocal
    {
        public string importe { get; set; }
        public string imp_loc_traslado { get; set; }
        public string tasa_de_traslado { get; set; }
    }

    public class CopadePreFactura
    {
        public CopadePreFacturaComprobante comprobante { get; set; }
        public CopadeAddenda Addenda { get; set; }
    }

    public class CopadePreFacturaComprobante
    {
        public string subtotal { get; set; }
        public string total { get; set; }
        public string moneda { get; set; }
        public IEnumerable<CopadeComprobanteConceptos> conceptos { get; set; }
        public CopadeComprobanteImpuestos impuestos { get; set; }

    }

    public class CopadeComprobanteConceptos
    {
        public string posicion { get; set; }
        public string descripcion { get; set; }
        public string importe { get; set; }
        public string cantidad { get; set; }
        public string unidad { get; set; }
        public string valorUnitario { get; set; }
    }

    public class CopadeComprobanteImpuestos
    {
        public string totalImpuestosRetenidos { get; set; }
        public string totalImpuestosTrasladados { get; set; }
        public CopadeImpuestosRetenciones retenciones { get; set; }
        public CopadeImpuestosTraslados traslados { get; set; }
    }

    public class CopadeNotasCredito
    {
        public IEnumerable<CopadeNotaCredito> notaCredito { get; set; }
    }

    public class CopadeNotaCredito
    {
        public string Tipo { get; set; }
        public string Concepto { get; set; }
        public string Cantidad { get; set; }
        public string Importe { get; set; }
        public string Iva { get; set; }
        public string Total { get; set; }
        public virtual Guid? Id { get; set; }
        public virtual bool EsNotaCreditoAgregada { get; set; }
        public virtual bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class CopadeAddenda
    {
        public CopadeAddendaPemex Addenda_Pemex { get; set; }
    }

    public class CopadeAddendaPemex
    {
        public string CONTRATO { get; set; }
        public string O_SURTIMIENTO { get; set; }
        public string N_ESTIMACION { get; set; }
        public string P_ESTIMACION { get; set; }
        public string N_ACREEDOR { get; set; }
        public string C_GESTOR { get; set; }
        public string DOSALMILLAR { get; set; }
        public string FINIQUITO { get; set; }
        public string POSICIONAP { get; set; }
        public string AUTORIZA { get; set; }
        public string EJERCICIO { get; set; }
        public string ENTRADA { get; set; }
        public string CEJECUTOR { get; set; }
        public string RECEPSAP { get; set; }
        public string PLAZO { get; set; }
        public string RFCPROVEEDOR { get; set; }
        public string REMESA { get; set; }
        public string NREMISION { get; set; }
        public string VUREGION { get; set; }
        public string FICHAE { get; set; }
        public string FICHAF { get; set; }
        public string MONEDA { get; set; }
        public string FONDO { get; set; }
        public string POSICIONF { get; set; }
        public string OCOMERCIAL { get; set; }
        public string SERVICIOG { get; set; }
        public string SERVICIOA { get; set; }
        public string CORREOPMI { get; set; }
    }
}
