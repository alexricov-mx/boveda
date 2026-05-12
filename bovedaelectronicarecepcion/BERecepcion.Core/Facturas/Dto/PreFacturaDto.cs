using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class PreFacturaDto
    {
        public Guid CopadeID { get; set; }
        public Guid OrganismID { get; set; }
        public string SapOrder { get; set; }
        public string Reception { get; set; }
        public string Exercise { get; set; }
        public string Ficha { get; set; }
        public string Creditor { get; set; }
        public string CreditorRfc { get; set; }
        public string Rfc { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string PreFacturaXML { get; set; }
        public string NotaCreditoXML { get; set; }
        public string CreditorNumber { get; set; }
        public string clave { get; set; }
        public string Center { get; set; }
        public DateTime functionary1signdate { get; set; }
        public DateTime functionary2signdate { get; set; }
        public DateTime ProviderEmailSendDate { get; set; }
    }
    
}
