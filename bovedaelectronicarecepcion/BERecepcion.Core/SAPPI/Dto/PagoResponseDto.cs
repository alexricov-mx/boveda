using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.SAPPI.Dto
{
    public class PagoResponseDto
    {
        public Guid ID { get; set; }
        public string Status { get; set; }
        public string Mensaje { get; set; }
    }
}
