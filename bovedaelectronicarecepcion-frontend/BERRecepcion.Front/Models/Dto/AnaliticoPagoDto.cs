using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
	public class AnaliticoPagoDto
	{
		public Guid AnaliticoPagoID { get; set; }
		public string Analitico { get; set; }
		public string Cedula { get; set; }
		public string Centro { get; set; }
		public string Contrato { get; set; }
		public string CveTransportista { get; set; }
		public string Ejercicio { get; set; }
		public string EsTRI { get; set; }
		public string Faltante { get; set; }
		public DateTime? FechaEmision { get; set; }
		public string IdAnalitico { get; set; }
		public string Iva { get; set; }
		public string Moneda { get; set; }
		public string NumAcreedor { get; set; }
		public string NumCliente { get; set; }
		public Guid OrganismID { get; set; }
		public string SubTotal { get; set; }
		public string Total { get; set; }
		public string FunctionaryEmail { get; set; }
		public DateTime? FunctionaryEmailedSendDate { get; set; }
		public string FunctionaryEmailReason { get; set; }
		public DateTime? FunctionarySignDate { get; set; }
		public DateTime? FunctionaryNotifyPemexDate { get; set; }
		public string FunctionaryFicha { get; set; }
		public string ProviderEmail { get; set; }
		public DateTime? ProviderEmailSendDate { get; set; }
		public string ProviderEmailReason { get; set; }
		public bool? IsCancel { get; set; }
		public DateTime? CancelDate { get; set; }
		public string CancelBy { get; set; }
		public string Addenda { get; set; }
		public string Impuestos { get; set; }
		public string PreFactura { get; set; }
		public string PreFacturaXML { get; set; }
		public string NotaCredito { get; set; }
		public string NotaCreditoXML { get; set; }
		public string Creditor { get; set; }
		public string CreditorRFC { get; set; }
		public virtual string clave { get; set; }
		public virtual APAddenda vAddenda { get; set; }
		public virtual APImpuestos vImpuestos { get; set; }
		public virtual APPreFactura vPreFactura { get; set; }
		public virtual ComprobanteBE Comprobante { get; set; }
		public virtual string FunctionarySignerName { get; set; }
		public virtual string FunctionaryCancelName { get; set; }
	}

	public class APAddenda
	{
		public APAddendaPemex Addenda_Pemex { get; set; }
	}

	public class APAddendaPemex
	{
		public string N_ACREEDOR { get; set; }
		public string EJERCICIO { get; set; }
		public string CLAVE_TRANSP { get; set; }
		public string A_RELACION { get; set; }
		public string ID_ANALITICO { get; set; }
		public string TIPO_PRODUCTO { get; set; }
		public string CEDULA { get; set; }
		public string CONTRATO_SIIC { get; set; }
		public string ANALITICO { get; set; }
	}

	public class APImpuestos
	{
		public string totalImpuestosRetenidos { get; set; }
		public string totalImpuestosTrasladados { get; set; }
		public APImpuestosRetenciones Retenciones { get; set; }
		public APImpuestosTraslados Traslados { get; set; }
	}

	public class APImpuestosRetenciones
	{
		public IEnumerable<APImpuestosRetencionesRetencion> Retencion { get; set; }
	}

	public class APImpuestosTraslados
	{
		public IEnumerable<APImpuestosTrasladosTraslado> Traslado { get; set; }
	}

	public class APImpuestosRetencionesRetencion
	{
		public string importe { get; set; }
		public string impuesto { get; set; }
	}

	public class APImpuestosTrasladosTraslado
	{
		public string importe { get; set; }
		public string impuesto { get; set; }
		public string tasa { get; set; }
	}

	public class APPreFactura
	{
		public APComprobante comprobante { get; set; }
		public CopadeComprobanteImpuestos impuestos { get; set; }
		public APAddenda Addenda { get; set; }
	}

	public class APComprobante
	{
		public string subtotal { get; set; }
		public string total { get; set; }
		public string moneda { get; set; }
		public IEnumerable<APComprobanteConceptos> conceptos { get; set; }
	}

	public class APComprobanteConceptos
	{
		public string cantidad { get; set; }
		public string? cve_producto { get; set; }
		public string descripcion { get; set; }
		public string importe { get; set; }
		public string unidad { get; set; }
		public string valorUnitario { get; set; }
	}

}
