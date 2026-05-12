using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class InvoiceProcessDto
    {
        public int status { get; set; }
        public List<ValidationError> validationErrors { get; set; }
        public InvoiceDto invoiceDto { get; set; }
    }
}
