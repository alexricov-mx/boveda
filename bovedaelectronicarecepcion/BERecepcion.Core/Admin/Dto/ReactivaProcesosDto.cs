using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class ReactivaProcesosDto
    {
		public String SAPOrderRP { get; set; }
		public String Name { get; set; }
		public String TypeDocument { get; set; }
		public String Type { get; set; }
		public String Clave { get; set; }
		public String Contract { get; set; }
		public String Usuario_Modificador { get; set; }
		public int valida { get; set; }
		public int Firmante { get; set; }
	}
}
