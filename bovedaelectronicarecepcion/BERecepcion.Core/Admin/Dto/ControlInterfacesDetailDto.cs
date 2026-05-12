using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class ControlInterfacesDetailDto
    {
		public Guid DetailID { get; set; }
		public Guid SAPID { get; set; }
		public DateTime Fecha { get; set; }
		public string Mensaje { get; set; }
		public string UsuarioModif { get; set; }
		public bool Activo { get; set; }
	}
}
