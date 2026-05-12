using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class NotaCreditoCFDIXMLDto
    {
        public Guid UUID { get; set; }
        public string OriginalXML { get; set; }
    }
}
