using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class InvoiceDto
	{
		public Guid InvoiceId {get; set;}
		public Guid DocumentoBEId { get; set; }
		public Guid UserId { get; set; }
		public string EmailProvider { get; set; }
		public DateTime EmailProviderSendDate { get; set; }
		public string Assignment { get; set; }
		public string CompensationDocument { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string Serie { get; set; }
		public string Folio { get; set; }
		public string TipoComprobante { get; set; }
	    public string Total { get; set; }
		public string Subtotal { get; set; }
		public string TotalImpuestosTrasladados { get; set; }
		public string TotalImpuestosRetenidos { get; set; }
		public Guid Uuid { get; set; }
		public string SapDocument { get; set; }
	    public bool IsCopade { get; set; }
		public string ElectronicReception { get; set; }
		public DateTime ReceptionDate { get; set; }
		public string RutaArchivo { get; set; }
		public string OriginalXML { get; set; }
		public string LastStatus { get; set; }
		public DateTime LastStatusDate { get; set; }
		public string Estatus { get; set; }
		public DateTime? FechaEmision { get; set; }
		public string ViaPago { get; set; }
		public string ImporteOriginal { get; set; }
		public string DiferencialCargo { get; set; }
		public string DiferencialAbono { get; set; }
		public Guid? DoctoRelacionado { get; set; }
		public virtual IEnumerable<InvoiceNotaCreditoDto> InvoiceNotaCredito { get; set; }
		public CartaPorte CartaPorte { get; set; }
		public string CFDIVersion { get; set; }
	}

	public class InvoiceFullDataDto
    {
		public InvoiceDto invoiceDto { get; set; }
		
		public List<InvoiceCxPDto> invoiceCxPDtos { get; set; }
    }

	public class InvoiceByReceptionDto
    {
		public string organismo { get; set; }
		public string reception { get; set; }
		public string ejercicio { get; set; }
		public InvoiceDto invoiceDto { get; set; }
    }

	public class InvoiceResultDto
    {
		public string Organismo { get; set; }
		public string IdDocumento { get; set; }
		public string RFCReceptor { get; set; }
		public InvoiceDto InvoiceDto { get; set; }
		public UsersDto User { get; set; }
		public IEnumerable<ValidationError> validationErrors { get; set; }
		public Guid InvoiceId { get; set; }
		public int Status { get; set; }
		public bool ExistedException { get; set; }
		public string ExceptionMessage { get; set; }
		public ComprobanteBE comprobante { get; set; }
		public string ViaPago { get; set; }
		public string DocumentoBEId { get; set; }
		public string OriginalXML { get; set; }
		public string Correo { get; set; }
		public IEnumerable<ComprobanteBE> notasCredito { get; set; }
		public IEnumerable<NotaCreditoCFDIXMLDto> notasCreditoCFDI { get; set; }
		public string ComprobanteBEString { get; set; }
		public string ComprobanteOriginal { get; set; }
		public string CFDIVersion { get; set; }
	}

	[Serializable]
	public class InvoiceAP
	{
		public Guid InvoiceId { get; set; }
		public Guid DocumentoBEId { get; set; }
		public Guid UserId { get; set; }
		public string EmailProvider { get; set; }
		public DateTime EmailProviderSendDate { get; set; }
		public string Assignment { get; set; }
		public string CompensationDocument { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string Serie { get; set; }
		public string Folio { get; set; }
		public string TipoComprobante { get; set; }
		public string Total { get; set; }
		public string Subtotal { get; set; }
		public string TotalImpuestosTrasladados { get; set; }
		public string TotalImpuestosRetenidos { get; set; }
		public Guid Uuid { get; set; }
		public string SapDocument { get; set; }
		public bool IsCopade { get; set; }
		public string ElectronicReception { get; set; }
		public DateTime ReceptionDate { get; set; }
		public string RutaArchivo { get; set; }
		public string OriginalXML { get; set; }
		public string LastStatus { get; set; }
		public DateTime LastStatusDate { get; set; }
		public string Estatus { get; set; }
		public DateTime? FechaEmision { get; set; }
		public string ViaPago { get; set; }
		public string ImporteOriginal { get; set; }
		public string DiferencialCargo { get; set; }
		public string DiferencialAbono { get; set; }
		public string Version { get; set; }
	}
}
