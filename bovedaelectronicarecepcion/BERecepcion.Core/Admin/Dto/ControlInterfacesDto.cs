using System;
using System.Collections.Generic;

namespace BERecepcion.Core.Admin.Dto
{
    public class ControlInterfacesDto
    {
        public Guid SAPID { get; set; }
        public string SAP { get; set; }
        public bool Status { get; set; }
        public int? StatusB { get; set; }
        public virtual IEnumerable<ControlInterfacesRolesDto> ControlInterfacesRoles { get; set; }
        public virtual ControlInterfacesDetailDto ControlInterfacesDetail { get; set; }
        public virtual bool? Activar { get; set; }
    }
}
