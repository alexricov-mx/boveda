using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BERRecepcion.Front.Models;

namespace BERRecepcion.Front.Models.Dto
{
    public class InvoiceValidateDto
    {
        public string claveOrganismo { get; set; }
        public string recepcion { get; set; }
        public string ejercicio { get; set; }
        public List<ValidationError> validationErrors { get; set; }
        public InvoiceDto invoiceDto { get; set; }
    }
}
