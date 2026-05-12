using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{
	public class ProfilesDto
	{
		public Guid ProfileID { get; set; }
		public string Name { get; set; }
		public bool? status { get; set; }
		public virtual IEnumerable<RolesCatalogoDto> RolesCatalogo { get; set; }
		public virtual IEnumerable<ProfilesRolesDto> ProfilesRoles { get; set; }
	}
}
