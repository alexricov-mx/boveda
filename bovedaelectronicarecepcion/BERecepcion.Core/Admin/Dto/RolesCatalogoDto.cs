using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
	public class RolesCatalogoDto
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
		public virtual List<SubMenuDto> SubMenu { get; set; }
        public RolesCatalogoDto()
        {
			this.SubMenu = new List<SubMenuDto>();
		}
	}
}
