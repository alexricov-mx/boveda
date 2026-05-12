using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class ValidacionCXPDto
    {
        public string Contrato { get; set; }
        public string Fecha { get; set; }
        public string Organismo { get; set; }
    }
    public class ValidaMiro_responseDto
    {
        public bool Status { get; set; }
        public string Mensaje { get; set; }
    }

    public class ValidacionMiroDto
    {
        public string Contrato { get; set; }
        public string Fecha { get; set; }
        public string Organismo { get; set; }
        public bool Status { get; set; }
        public string Mensaje { get; set; }

    }

    //"{\"ValidaMiro_response\":{\"Status\":true,\"Mensaje\":\"\"}}"
}
