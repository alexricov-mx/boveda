using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
   public class InvoiceCxPList
    {
        public Guid InvoiceId { get; set; }
        public string clave { get; set; }
        public string Exercise { get; set; }
        public String Reception { get; set; }
        public String SapOrder { get; set; }
        public String Serie { get; set; }
        public String Folio { get; set; }
        public Guid Usuario { get; set; }
        public Guid Uuid { get; set; }
        public String Assignment { get; set; }
        public String Total { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ReceptionDate { get; set; }
        public String Creditor { get; set; }
        public DateTime CxpSendDate { get; set; }
        public String res { get; set; }
#nullable enable
        public String? DocumentoSAP { get; set; }
#nullable disable
    }
}
