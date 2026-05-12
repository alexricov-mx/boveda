using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class FacturaEmailCopade
    {
        public IEnumerable<ValidationError> validation { get; set; }
        public UsersDto user { get; set; }
        public string copade { get; set; }
        public string factura { get; set; }
    }
}
