using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Correos.Dto
{
    public class FacturaEmailCopade
    {
        public virtual IEnumerable<ValidationError> validation { get; set; }
        public UsersDto user { get; set; }
        public string copade { get; set; }
        public string factura { get; set; }
    }
}
