using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class InvoiceEstatusDto
    {
        public Guid InvoiceId { get; set; }
        public string Estatus { get; set; }
        public string LastStatus { get; set; }
        public bool result { get; set; }
    }
}
