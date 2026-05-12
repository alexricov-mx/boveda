using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.Instrucciones.Dto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Consulta.Dto
{
    public class ExpedienteEViewModel
    {
        public string Title { get; set; }
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
