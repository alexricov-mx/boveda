using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class PendingInvoiceDto
    {
        public int Id { get; set; }
        public string Reception { get; set; }
        public Guid UserId { get; set; }
        public string XmlContent { get; set; }
        public DateTime EntryDate { get; set; }
        public string ClaveOrganismo { get; set; }
        public bool EsCopade { get; set; }
        public bool EsDocumental { get; set; }
        public List<string> NotasCredito { get; set; }
        public int tipo { get; set; }
    }

    public class PendingEmailDto
    {
        public Guid InvoiceId { get; set; }
        public Guid DocumentoBEId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public Guid UUID { get; set; }
        public string Reception { get; set; }
        public string Estatus { get; set; }
        public string LastStatus { get; set; }
    }
}
