using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class InfoAplicativoModel
    {
        public string Aplicativo { get; set; }
        public string BuildId { get; set; }
        public string Version { get; set; }
        public string Ambiente { get; set; }
        public string Mensaje { get; set; }
    }
}
