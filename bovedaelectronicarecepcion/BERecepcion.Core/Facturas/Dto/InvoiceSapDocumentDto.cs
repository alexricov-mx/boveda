using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class InvoiceSapDocumentDto
    {
        public Guid InvoiceId { get; set; }
        public string SapDocument { get; set; }
        public bool result { get; set; }
    }
}
