using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class NotificacionFacturaEmailDto
    {
        public IEnumerable<ValidationError> validationError { get; set; }
        public string copade { get; set; }
        public string factura { get; set; }
        public UsersDto user { get; set; }
        public string result { get; set; }
        public string reception { get; set; }
    }
}
