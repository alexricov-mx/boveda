using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class DocumentoFirmadoDto
    {
		public Guid DocumentoFirmadoID { get; set; }
		public int paqueteId { get; set; }
		public Guid DocumentoBEId { get; set; }
		public int documentoId { get; set; }
		public int usuarioId { get; set; }
		public string DocumentType { get; set; }
		public int Orden { get; set; }
		public Guid? usuarioBEId { get; set; }
		public DateTime? FechaFirma { get; set; }
	}
}
