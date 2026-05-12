using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class DesvioDto
    {
        public Guid DesvioID { get; set; }
        public Guid OrganismID { get; set; }
        public DateTime FechaDesvio { get; set; }
        public string Contract { get; set; }
        public string SapOrder { get; set; }
        public string Reception { get; set; }
        public string DocumentType { get; set; }
        public string Signer { get; set; }
        public string SignerNew { get; set; }
        public string Usuario_Modificador { get; set; }
        public string Justificacion { get; set; }
        public bool Activo { get; set; }
    }
}
