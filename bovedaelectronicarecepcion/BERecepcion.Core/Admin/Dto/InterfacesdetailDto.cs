using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class InterfacesdetailDto
    {
        public Guid DetailID { get; set; }
        public Guid SAPID { get; set; }
        public DateTime Fecha { get; set; }
        public String Mensaje { get; set; }
        public String UsuarioModif { get; set; }
        public bool Activo { get; set; }
    }
}
