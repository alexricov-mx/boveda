using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.SAPPI.Dto
{
    public class OSLiberacionVPDto
    {
        public string Contrato { get; set; }
        public string OrdenSap { get; set; }
        public string OrdenSiaf { get; set; }
        public string FechaLiberacion { get; set; }
        public string Status { get; set; }
    }
}
