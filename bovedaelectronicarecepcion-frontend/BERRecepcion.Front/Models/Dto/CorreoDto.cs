using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class CorreoDto
    {
        public string To { get; set; }
        //public string From { get; set; }
        //public string Bcc { get; set; }
        //public string Priority { get; set; }
        public string Subject { get; set; }
        //public string Body { get; set; }
        //public string IsBodyHtml { get; set; }
        public string? Accion { get; set; }
        public string? userName { get; set; }
        public string? Nombre { get; set; }
        public string? Contract { get; set; }
        public string? SapOrder { get; set; }
        public string? Estimacion { get; set; }
        public string? Reception { get; set; }
        public string? Copade { get; set; }
        public string? AnaliticoPago { get; set; }
        public string? ProgramaPago { get; set; }
        public string? ListaPago { get; set; }
        public List<string> ListTo { get; set; }
    }
}
