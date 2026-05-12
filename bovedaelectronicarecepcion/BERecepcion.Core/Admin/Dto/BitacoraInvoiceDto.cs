using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class BitacoraInvoiceDto
    {
        public string clave { get; set; }
        public string reception { get; set; }
        public string userId { get; set; }
        public int medioProceso { get; set; }
        public Guid documentBeId { get; set; }

        public BitacoraInvoiceDto(string _clave, string _reception, string _userId, int _medioProceso)
        {
            clave = _clave;
            reception = _reception;
            userId = _userId;
            medioProceso = _medioProceso;
        }

        public BitacoraInvoiceDto()
        {

        }
    }
}
