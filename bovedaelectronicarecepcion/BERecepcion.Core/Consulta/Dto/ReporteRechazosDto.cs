using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Consulta.Dto
{
    public class ReporteRechazosDto
    {
        public int id { get; set; }
        public string RFCAcreedor { get; set; }
        public string RazonSocial { get; set; }
        public string RFCReceptor { get; set; }
        public string Organismo { get; set; }
        public DateTime FechaRechazo { get; set; }
        public string Correo { get; set; }
        public List<string> MotivoRechazo { get; set; }
        public string Documento { get; set; }
        public Guid lote { get; set; }
    }

    public class ReporteRechazosItemDto
    {
        public int id { get; set; }
        public Guid lote { get; set; }
        public string RFCAcreedor { get; set; }
        public string RazonSocial { get; set; }
        public string RFCReceptor { get; set; }
        public string Organismo { get; set; }
        public DateTime FechaRechazo { get; set; }
        public string Correo { get; set; }
        public string MotivoRechazo { get; set; }
        public string Documento { get; set; }
    }
}
