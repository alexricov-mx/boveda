using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class BitacoraInvoiceDto
    {
        public string clave { get; set; }
        public string reception { get; set; }
        public string userId { get; set; }
        public int medioProceso { get; set; }

        public BitacoraInvoiceDto(string _clave, string _reception, string _userId, int _medioProceso)
        {
            this.clave = _clave;
            this.reception = _reception;
            this.userId = _userId;
            this.medioProceso = _medioProceso;
        }

        public BitacoraInvoiceDto()
        {

        }
    }
}
