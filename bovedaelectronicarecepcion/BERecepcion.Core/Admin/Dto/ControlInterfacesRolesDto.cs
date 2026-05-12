using System;

namespace BERecepcion.Core.Admin.Dto
{
    public class ControlInterfacesRolesDto
    {
        public Guid? sapid { get; set; }
        public Guid? rolid { get; set; }
        public bool? status { get; set; }

        public virtual string Descripcion { get; set; }
    }
}
