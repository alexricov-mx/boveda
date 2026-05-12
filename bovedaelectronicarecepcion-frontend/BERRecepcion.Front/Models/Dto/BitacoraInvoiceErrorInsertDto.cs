using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class BitacoraInvoiceErrorInsertDto
    {
        public string reception { get; set; }
        public string UserId { get; set; }
        public int medioProceso { get; set; }
        public IEnumerable<ValidationError> errors { get; set; }
    }
}
