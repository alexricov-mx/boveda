using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class InvoiceCxPDto
    {
        public Guid CxPId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid UserId { get; set; }
        public string Res { get; set; }
    }

    public class InvoiceCxPListDto
    {
        public Guid InvoiceId { get; set; }
        public IEnumerable<InvoiceCxPDto> CxPs { get; set; }
    }
}
