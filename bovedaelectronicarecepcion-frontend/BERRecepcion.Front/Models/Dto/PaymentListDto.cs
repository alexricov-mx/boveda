using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class PaymentListDto
    {
        public Guid PaymentListID { get; set; }
        public Guid OrganismID { get; set; }
        public string ListaPago_Id { get; set; }
        public DateTime ReceptionDate { get; set; }
        public DateTime Date { get; set; }
        public string Authorizes { get; set; }
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
        public virtual IEnumerable<ProgramaListaEnumDto> LPP { get; set; }
    }

    public class ProgramaListaEnumDto
    {
        public Guid PaymentListID { get; set; }
        public bool Status { get; set; }
        public string ListaPago_Id { get; set; }

    }
}
