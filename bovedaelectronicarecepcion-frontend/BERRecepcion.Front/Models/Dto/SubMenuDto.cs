using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
	public class SubMenuDto
	{
		public Guid RolId { get; set; }
		public string Categoria { get; set; }
		public string Rol { get; set; }
		public string Descripcion { get; set; }
		public string Controller { get; set; }
		public string Action { get; set; }
		public int RolOrden { get; set; }
		public int CategoriaOrden { get; set; }
		public bool Activo { get; set; }
		public string Icono { get; set; }
		public virtual bool IsSelected { get; set; }
	}
}
