using System;
using System.Collections.Generic;

namespace BERRecepcion.Front.Models.Dto
{
	public class SupplyOrderDto
	{
		public Guid SupplyOrderID { get; set; }
		public Guid OrganismID { get; set; }
		public string Contract { get; set; }
		public string DocumentType { get; set; }
		public string SAPOrder { get; set; }
		public string SIAFOrder { get; set; }
		public string Type { get; set; }
		public string Creditor { get; set; }
		public string CreditorNumber { get; set; }
		public string CreditorRFC { get; set; }
		public string Total { get; set; }
		public string Currency { get; set; }
		public string MadeBy { get; set; }
		public string Representative { get; set; }
		public string Signer { get; set; }
		public bool? EsTRI { get; set; }
		public bool? Historical { get; set; }
		public DateTime? ReceptionDate { get; set; }
		public string AdministratorEmail { get; set; }
		public DateTime? AdministratorEmailSendDate { get; set; }
		public string AdministratorEmailReason { get; set; }
		public DateTime? AdministratorSignDate { get; set; }
		public string FunctionaryEmail { get; set; }
		public DateTime? FunctionaryEmailSendDate { get; set; }
		public string FunctionaryEmailReason { get; set; }
		public DateTime? FunctionarySignDate { get; set; }
		public DateTime? FunctionaryNotifyPemexDate { get; set; }
		public string FunctionaryFicha { get; set; }
		public string ProviderEmail { get; set; }
		public DateTime? ProviderEmailSendDate { get; set; }
		public string ProviderEmailReason { get; set; }
		public DateTime? ProviderSignDate { get; set; }
		public DateTime? ProviderNotifyPemexDate { get; set; }
		public bool? IsFullSigned { get; set; }
		public bool? IsCancel { get; set; }
		public DateTime? CancelDate { get; set; }
		public string CancelBy { get; set; }
		public DateTime? LiberacionVPDate { get; set; }

		public virtual string OrganismName { get; set; }
		public virtual string OrganismClave { get; set; }
		public virtual string FunctionarySignerName { get; set; }
		public virtual string ProviderSignerName { get; set; }
		public virtual string FunctionaryCancelName { get; set; }
        public virtual IEnumerable<DateHelperModel> Seguimiento { get; set; }
    }

	public class SupplyOrderDetalle
	{
		public string Cantidad { get; set; }
		public string CorpIndicador { get; set; }
		public string Descripcion { get; set; }
		public string Importe { get; set; }
		public string Partida { get; set; }
		public string PrecioUnitario { get; set; }
		public string Unidad { get; set; }
	}
}
