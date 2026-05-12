using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace BERecepcion.Core.Consulta.Dto
{
    public class ReportePaymentScheduleDto
    {
        public string ConsolidacionID { get; set; }
        public DateTime FechaPrograma { get; set; }
        public DateTime FechaPago { get; set; }
        public bool FuncStatus { get; set; }
        public string Organismo { get; set; }
        public Guid OrganismID { get; set; }
        public string ProgramaPago_Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime ReceptionDate { get; set; }
        public DateTime ScheduleDate { get; set; }
    }

    public class ReportePaymentListDto
    {
        public string Organismo { get; set; }
        public Guid OrganismID { get; set; }
        public string ListaPago_Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime ReceptionDate { get; set; }
    }

    public class ReportePaymentList
    {
        public string Organismo { get; set; }
        public Guid OrganismID { get; set; }
        public string ListaPago_Id { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string PositionP { get; set; }
        public string To { get; set; }
        public Guid PaymentListID { get; set; }
        public string Authorizes { get; set; }
        public string PositionA { get; set; }
        public string DetallePep { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public string Found { get; set; }
        public string ValueDate { get; set; }
        public string DetalleId { get; set; }
        public string Amount { get; set; }
        public IEnumerable<ReportePaymentList> ListPayment { get; set; }
        public IEnumerable<ReportePaymentList> DetalePep { get; set; }
    }
}
