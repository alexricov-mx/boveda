using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class InvoiceSaveGralDto
    {
        public InvoiceDto invoiceDto { get; set; }
        public string invoiceXML { get; set; }
        public InvoiceCxPDto invoiceCxPDto { get; set; }
        public List<InvoiceNotaCreditoDto> invoiceNotasCreditoDto { get; set; }
        public List<string> InvoiceNotasCreditoXML { get; set; }
    }
}
