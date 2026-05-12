using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class EstadoFacturaDto
    {
        public Guid DocumentoId { get; set; }
        public string Documento { get; set; }
        public string Origen { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public DateTime FechaFactura { get; set; }
        public string Comentario { get; set; }
        public string Folio { get; set; }
        public Guid? UUID { get; set; }
        public string Total { get; set; }
        public string CorreoEnviado { get; set; }
        public string SapOrder { get; set; }
        public string Reception { get; set; }
        public string Organismo { get; set; }
    }
}
