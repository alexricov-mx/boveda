using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.SAPPI.Dto
{
    public class OSResponseItemDto
    {
        public string ORGANISMO { get; set; }
        public string TIPO { get; set; }
        public string CONTRATO { get; set; }
        public string ORDEN_SAP { get; set; }
        public string STATUS { get; set; }
    }

    public class CopadeResponseItemDto
    {
        public string ORGANISMO { get; set; }
        public string TIPO { get; set; }
        public string CONTRATO { get; set; }
        public string ORDEN_SAP { get; set; }
        public string STATUS { get; set; }
    }
}
