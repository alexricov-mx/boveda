using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class UnionReactivarProcesosDto
    {
        public virtual ReactivaProcesosDto ReactivaProcesosFirma { get; set; }
        public virtual IEnumerable<ReactivaProcesosResponseDto>? ReactivaProcesosDatos { get; set; }
    }
}
