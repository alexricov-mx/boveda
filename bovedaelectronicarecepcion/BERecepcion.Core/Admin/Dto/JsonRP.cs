using BERecepcion.Core.SAPPI.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
	public class JsonRP
	{
		public String SAPOrderRP { get; set; }
		public String Name { get; set; }
		public String TypeDocument { get; set; }
		public String Type { get; set; }
		public String Clave { get; set; }		
		public String Contract { get; set; }
        public string Result { get; set; }
        public String Usuario_Modificador { get; set; }
		public int valida { get; set; }
		public int Firmante { get; set; }
		public OSResponseDto oSResponse { get; set; }
	}
}
