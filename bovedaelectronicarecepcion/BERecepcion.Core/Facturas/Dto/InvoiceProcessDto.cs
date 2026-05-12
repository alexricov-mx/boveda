using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class InvoiceProcessDto
    {
        public int status { get; set; }
        public List<ValidationError> validationErrors { get; set; }
        public InvoiceDto invoiceDto { get; set; }
    }
}
