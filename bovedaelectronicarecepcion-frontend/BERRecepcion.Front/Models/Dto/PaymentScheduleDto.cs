using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
	public class PaymentScheduleDto
	{
		public Guid PaymentScheduleID { get; set; }
		public Guid OrganismID { get; set; }
		public string ProgramaPago_Id { get; set; }
		public DateTime PaymentDate { get; set; }
		public DateTime ReceptionDate { get; set; }
		public DateTime ScheduleDate { get; set; }
		public string Authorizes { get; set; }
		public string Mail { get; set; }
		public string PositionAuthorizes { get; set; }
		public string PositionTo { get; set; }
		public string To { get; set; }
		public string TokenAuthorizes { get; set; }
		public string TokenTo { get; set; }
		public DateTime? EmailAuthorizesSendDate { get; set; }
		public DateTime? AuthorizesSignDate { get; set; }
		public DateTime? EmailToSendDate { get; set; }
		public bool? IsFullSigned { get; set; }
		public bool? IsCancel { get; set; }
		public DateTime? CancelDate { get; set; }
		public string CancelBy { get; set; }
		public string Detalle { get; set; }
		public string DetallePep { get; set; }
		public virtual IEnumerable<P_Detalle> vDetalle { get; set; }
		public virtual IEnumerable<P_DetallePep> vDetallePep { get; set; }
		public virtual string OrganismClave { get; set; }
		public virtual IEnumerable<ProgramaPagosEnumDto> CPP { get; set; }
	}
	public class P_Detalle
	{
		public string CompensationDocument { get; set; }
		public string DocumentSap { get; set; }
	}

	public class P_DetallePep
	{
		public string Amount { get; set; }
		public string Currency { get; set; }
		public string Found { get; set; }
		public string Type { get; set; }
		public string ValueDate { get; set; }
	}

	public class ProgramaPagosEnumDto
	{
		public Guid PaymentScheduleID { get; set; }
		public bool Status { get; set; }
		public string ProgramaPago_Id { get; set; }

	}

}
