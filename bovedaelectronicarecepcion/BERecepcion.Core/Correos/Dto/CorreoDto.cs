using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Correos.Dto
{
    public class CorreoDto
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Accion { get; set; }
        public string userName { get; set; }
        public string Nombre { get; set; }
        public string Contract { get; set; }
        public string SapOrder { get; set; }
        public string Estimacion { get; set; }
        public string Reception { get; set; }
        public string Copade { get; set; }
        public string AnaliticoPago { get; set; }
        public string ProgramaPago { get; set; }
        public string ListaPago { get; set; }
    }
}
