using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class RecepcionElectronicaPDto
    {
        public Guid UUid { get; set; }
        public Guid PagoUUID { get; set; }
        public string xml { get; set; }
        public bool status { get; set; }
        public InvoiceDto invoice { get; set; }
        public ComprobanteBE comprobante { get; set; }
    }
}
