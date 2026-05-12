using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class InvoiceSapDocumentDto
    {
        public Guid InvoiceId { get; set; }
        public string SapDocument { get; set; }
        public bool result { get; set; }
    }
}
