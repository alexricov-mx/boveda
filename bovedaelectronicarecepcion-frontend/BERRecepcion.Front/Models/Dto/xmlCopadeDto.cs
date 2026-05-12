using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class xmlCopadeDto
    {
        public Guid CopadeID { get; set; }
        public bool Status { get; set; }
        public string Reception { get; set; }
        public string clave { get; set; }
        public string NotaCreditoXML { get; set; }
        public string CreditorNumber { get; set; }
    }
}
