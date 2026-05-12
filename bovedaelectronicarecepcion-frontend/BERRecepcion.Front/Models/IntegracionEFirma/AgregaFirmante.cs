using System.Collections.Generic;
using System;
using static BERRecepcion.Front.Models.CrearPaquete;

namespace BERRecepcion.Front.Models.IntegracionEFirma
{
    public class AgregaFirmante
    {
        public Guid? IdCorrelacion { get; set; }
        public IEnumerable<Firmante> Firmantes { get; set; } = Array.Empty<Firmante>();
    }
}
