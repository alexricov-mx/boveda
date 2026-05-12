using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
	public class AltaContratosDto
	{
		public Guid AltaContratoID { get; set; }
		public string Contract { get; set; }
		public DateTime Fecha_Alta { get; set; }
		public string Firma1 { get; set; }
		public string Suplente1 { get; set; }
		public string Firma2 { get; set; }
		public string Suplente2 { get; set; }
		public string Usuario_alta { get; set; }
        public bool Activo { get; set; }
        public virtual string Firma1Name { get; set; }
		public virtual string Suplente1Name { get; set; }
		public virtual string Firma2Name { get; set; }
		public virtual string Suplente2Name { get; set; }
#nullable enable
        public string? status { get; set; }
#nullable disable
    }
}
