using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
	public class PrefacturaAPDto
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
		public virtual IEnumerable<PreXmlMasAPDto> prefacturasAPXML { get; set; }

	}
	public class PreXmlMasAPDto
	{
		public Guid? AnaliticoPagoID { get; set; }
		public bool Status { get; set; }
		public string PreFacturaXML { get; set; }
		public string NotaCreditoXML { get; set; }

	}
}
