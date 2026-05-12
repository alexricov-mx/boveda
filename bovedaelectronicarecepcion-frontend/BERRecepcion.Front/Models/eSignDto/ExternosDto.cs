using BERRecepcion.Front.Models.eSignDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{
    public class ExternosDto
    {
        public Paquete paquete { get; set; }
        public DocumentoRepositorio documentoRepositorio { get; set; }
        public PaqueteFirmante paqueteFirmante { get; set; }
        public PaqueteDocumentoDetalleDto paqueteDocumento { get; set; }
        public UsuarioDto usuario { get; set; }
        public UnidadOrgUsuario usuarioRefOrg { get; set; }
        public DocumentoFirmaBE documentoFirmaBE { get; set; }
        public estatusPaquete estatusPaquete { get; set; }
        public DocumentoFirma documentoFirma { get; set; }
        public SupplyOrderDto supplyOrder { get; set; }
        public CopadeDto copade { get; set; }
        public AnaliticoPagoDto analiticoPago { get; set; }
        public SOEstimationDto sOEstimation { get; set; }
        public ReceptionDto reception { get; set; }
        public PaymentScheduleDto paymentSchedule { get; set; }
        public PaymentListDto paymentList { get; set; }
        public Guid? GUIDPaquete { get; set; }
        public Guid? GUIDocumento { get; set; }
        public string Certificado { get; set; }
        public string UrlQR { get; set; }
    }
}
