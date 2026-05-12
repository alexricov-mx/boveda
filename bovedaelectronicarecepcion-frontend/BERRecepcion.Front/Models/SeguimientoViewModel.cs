using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class SeguimientoViewModel
    {
        public ExpedienteEViewModel Expediente { get; set; }
        public List<DateHelperModel> Dates { get; set; }
    }
}
