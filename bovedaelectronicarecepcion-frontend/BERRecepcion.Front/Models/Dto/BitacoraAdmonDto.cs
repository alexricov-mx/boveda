using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{    
	public class BitacoraAdmonDto
	{
		public long Secuencia { get; set; }
		public string Evento { get; set; }
		public string Usuario { get; set; }
		public string Descripcion { get; set; }
		public DateTime FechaCambio { get; set; }
	}
}