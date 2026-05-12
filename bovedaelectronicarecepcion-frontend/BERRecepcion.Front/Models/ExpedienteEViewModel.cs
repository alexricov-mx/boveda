using BERRecepcion.Front.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class ExpedienteEViewModel
    {
        public SupplyOrderDto SupplyOrder { get; set; }
        public SOEstimationDto SOEstimation { get; set; }
        public CopadeDto Copade { get; set; }
        public AnaliticoPagoDto AnaliticoPago { get; set; }
        public InvoiceDto Invoice { get; set; }
        public IEnumerable<InvoiceNotaCreditoDto> NotasCredito { get; set; }
        public RecepcionElectronicaPDto Pagos { get; set; }
        public PaymentScheduleDto PaymentSchedule { get; set; }
        public PaymentListDto PaymentList { get; set; }        
        public string RedirectAction { get; set; }
    }
}
