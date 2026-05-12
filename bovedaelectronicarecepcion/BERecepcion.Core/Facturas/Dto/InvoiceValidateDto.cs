using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class InvoiceValidateDto
    {
        public string claveOrganismo { get; set; }
        public string recepcion { get; set; }
        public string ejercicio { get; set; }
        public List<ValidationError> validationErrors { get; set; }
        public InvoiceDto invoiceDto { get; set; }
        public Conceptos conceptos { get; set; }
        public Complemento complemento { get; set; }
    }
}
