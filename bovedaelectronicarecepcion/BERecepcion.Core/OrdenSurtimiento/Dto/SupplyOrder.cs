using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.OrdenSurtimiento.Dto
{
    public class SupplyOrder
    {
        public Guid SupplyOrderId { get; set; }
        public string OrganismId { get; set; }
        public string Contract { get; set; }
        public string DocumentType { get; set; }
        public string SapOrder { get; set; }
        public string SiafOrder { get; set; }
        public string Type { get; set; }
        public string Creditor { get; set; }
        public string CreditorNumber { get; set; }
        public string CreditorRfc { get; set; }
        public string Total { get; set; }
        public string Currency { get; set; }
        public string MadeBy { get; set; }
        public string Representative { get; set; }
        public string Signer { get; set; }
        public string EsTRI { get; set; }
        public string Historical { get; set; }
        public DateTime ReceptionDate { get; set; }
    }
}
