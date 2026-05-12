using BERecepcion.Core.Admin.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class NotificacionFacturaEmailDto
    {
        public IEnumerable<ValidationError> validationError { get; set; }
        public UsersDto user { get; set; }
        public string result { get; set; }
        public string reception { get; set; }
    }
}
