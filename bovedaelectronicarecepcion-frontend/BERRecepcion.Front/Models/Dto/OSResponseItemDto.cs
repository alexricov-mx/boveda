using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class OSResponseItemDto
    {
        public string? ORGANISMO { get; set; }
        public string? TIPO { get; set; }
        public string? CONTRATO { get; set; }
        public string? ORDEN_SAP { get; set; }
        public string? STATUS { get; set; }
    }
}
