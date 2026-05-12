using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class DataTransferObject
    {
        public int IdCreador { get; set; }
        public DateTime FechaCreacion { get; set; }

        public int? IdModificador { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
