using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class InvoiceSaveGralDto
    {
        public InvoiceDto invoiceDto { get; set; }
        public InvoiceCxPDto invoiceCxPDto { get; set; }
        public List<InvoiceNotaCreditoDto> invoiceNotasCreditoDto { get; set; }
    }
}
