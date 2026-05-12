using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class FacturaPdfDto
    {
        public ComprobanteDto[] data { get; set; }
        public IEnumerable<string> result { get; set; }
        public string UsuarioModificador { get; set; }
        public ComprobanteBE comprobante { get; set; }
    }
}
