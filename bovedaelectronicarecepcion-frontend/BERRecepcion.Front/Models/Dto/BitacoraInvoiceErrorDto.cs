using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class BitacoraInvoiceErrorDto
    {
        public int id { get; set; }
        public int medioProceso { get; set; }
        public string clave { get; set; }
        public Guid userId { get; set; }
        public DateTime fecha { get; set; }
        public string reception { get; set; }
        public string mensaje { get; set; }
        public Guid lote { get; set; }
        

        public BitacoraInvoiceErrorDto(int _id, string _clave, string _reception, Guid _userId, int _medioProceso, DateTime _fecha, string _mensaje, Guid _lote)
        {
            id = _id;
            clave = _clave;
            reception = _reception;
            userId = _userId;
            medioProceso = _medioProceso;
            fecha = _fecha;
            mensaje = _mensaje;
            lote = _lote;
        }

        public BitacoraInvoiceErrorDto()
        {

        }
    }
}
