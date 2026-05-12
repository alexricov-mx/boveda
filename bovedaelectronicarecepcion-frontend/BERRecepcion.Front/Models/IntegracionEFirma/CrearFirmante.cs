using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class CrearFirmante
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rfc { get; set; } = string.Empty;
        public int? Ficha { get; set; }
        public int? Extension { get; set; }
    }
}
