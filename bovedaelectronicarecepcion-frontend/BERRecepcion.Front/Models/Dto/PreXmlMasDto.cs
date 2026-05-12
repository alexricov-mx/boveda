using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class PreXmlMasDto
    {
        public Guid CopadeID { get; set; }
        public string Copade { get; set; }
        public bool Status { get; set; }
        public string PreFacturaXML { get; set; }
        public string NotaCreditoXML { get; set; }
        public virtual IEnumerable<PreXmlMasDto> CopadesID { get; set; }
    }
}
