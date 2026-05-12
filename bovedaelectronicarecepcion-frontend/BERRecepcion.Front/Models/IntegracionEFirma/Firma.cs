using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class Firma
    {
        public int? IdDocumento { get; set; }
        public Guid? IdCorrelacion { get; set; }
        public string Hash { get; set; } = string.Empty;
    }
}
