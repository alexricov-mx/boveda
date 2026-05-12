using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class ValidationError
    {
        public int id { get; set; }
        public string clave { get; set; }
        public string descripcion { get; set; }
        public string documento { get; set; }
        public bool esTerminal { get; set; }    //0 - permite reintento 1 - no hay reintento
        public bool activo { get; set; }
        public string mensaje { get; set; }
    }
}
