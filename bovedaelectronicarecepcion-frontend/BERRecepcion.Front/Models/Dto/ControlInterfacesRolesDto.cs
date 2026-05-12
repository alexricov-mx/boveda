using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class ControlInterfacesRolesDto
    {
        public Guid? sapid { get; set; }
        public Guid? rolid { get; set; }
        public bool? status { get; set; }

        public virtual string Descripcion { get; set; }
    }
}
