using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace BERRecepcion.Front.Models
{
    public class CrearPaquete
    {
        public Guid IdCorrelacion { get; set; } = Guid.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool? EnProcesoFirma { get; set; }

        public int? TotalFirmantes { get; set; } = 1;

        public IEnumerable<Documento> Documentos { get; set; } = Array.Empty<Documento>();
        public IEnumerable<Firmante> Firmantes { get; set; } = Array.Empty<Firmante>();                
        public class Documento
        {
            public Guid IdCorrelacion { get; set; } = Guid.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public string CodigoTipoDocumento { get; set; } = string.Empty;         
        }

        public class Firmante
        {
            public int IdUsuario { get; set; }
            public string? Figura { get; set; }
        }
    }
}
