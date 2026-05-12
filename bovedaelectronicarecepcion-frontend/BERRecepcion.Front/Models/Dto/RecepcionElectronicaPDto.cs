using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class RecepcionElectronicaPDto
    {
        public Guid UUid { get; set; }
        public string xml { get; set; }
        public bool status { get; set; }
        public InvoiceDto invoice { get; set; }
        public ComprobanteBE comprobante { get; set; }
    }
}
