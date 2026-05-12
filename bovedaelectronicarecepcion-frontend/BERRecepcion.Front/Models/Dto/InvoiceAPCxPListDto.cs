using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class InvoiceAPCxPListDto
    {
        public Guid InvoiceId { get; set; }
        public string clave { get; set; }
        public string Analitico { get; set; }
        public string Cedula { get; set; }
        public string Centro { get; set; }
        public string Contrato { get; set; }
        public string CveTransportista { get; set; }
        public string idAnalitico { get; set; }
        public string Ejercicio { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public Guid Usuario { get; set; }
        public Guid Uuid { get; set; }
        public string Assignment { get; set; }
        public string Total { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ReceptionDate { get; set; }
        public DateTime CxpSendDate { get; set; }
        public string res { get; set; }
    }
}
