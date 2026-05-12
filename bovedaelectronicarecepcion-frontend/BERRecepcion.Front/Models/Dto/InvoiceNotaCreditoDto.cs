using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class InvoiceNotaCreditoDto
    {
		public Guid NotaCreditoId { get; set; }
		public Guid InvoiceId { get; set; }
		public Guid DocumentoBEId { get; set; }
		public bool? IsCopade { get; set; }
		public Guid? Uuid { get; set; }
		public string Serie { get; set; }
		public string Folio { get; set; }
		public string Iva { get; set; }
		public string Amount { get; set; }
		public string Total { get; set; }
		public DateTime ReceptionDate { get; set; }
		public string OriginalXML { get; set; }
		public DateTime NotaCreditoDate { get; set; }
		public string Descripcion { get; set; }
		public string Subtotal { get; set; }
	}

	public class InvoiceNotaCreditoByReceptionDto
	{
		public string reception { get; set; }
		public InvoiceNotaCreditoDto invoiceNotaCreditoDto { get; set; }
	}
}
