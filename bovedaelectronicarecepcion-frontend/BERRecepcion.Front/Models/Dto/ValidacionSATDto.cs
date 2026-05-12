using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class ValidacionSATDto
    {
        public string RFCEmisor { get; set; }
        public string RFCReceptor { get; set; }
        public string Total { get; set; }
        public Guid UUID { get; set; }
        public string CodigoEstatus { get; set; }
        public string Estado { get; set; }
        public string EsCancelable { get; set; }
        public string EstatusCancelacion { get; set; }
        public string ValidacionEFOS { get; set; }
    }
}
