using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class BitacoraAdmonDto
    {
		public long Secuencia { get; set; }
		public string Evento { get; set; }
		public string Usuario { get; set; }
		public DateTime FechaCambio { get; set; }
		public string Descripcion { get; set; }
	}
}
