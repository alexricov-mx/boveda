using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BERecepcion.Core.IntegracionEFirma.CrearPaquete;

namespace BERecepcion.Core.IntegracionEFirma
{
    public class AgregaFirmante
    {   
        public Guid? IdCorrelacion { get; set; }
        public IEnumerable<Firmante> Firmantes { get; set; } = Array.Empty<Firmante>();
    }
}
