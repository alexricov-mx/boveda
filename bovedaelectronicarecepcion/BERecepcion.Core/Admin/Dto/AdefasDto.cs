using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class AdefasDto
    {
        public Guid AdefaID { get; set; }
        public Guid OrganismID { get; set; }
        public string AnhioFactura { get; set; }
        public string InicioVentana { get; set; }
        public string FinVentana { get; set; }
        public bool Activo { get; set; }

        public virtual string OrganismClave { get; set; }
    }
}
