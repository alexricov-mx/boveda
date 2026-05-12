using BERRecepcion.Front.Models.Dto;
using System;

namespace BERRecepcion.Front.Models.IntegracionEFirma
{
    public class Externos2Dto
    {        
        public CrearPaquete paquete { get; set; }
        public CrearPaqueteResult paqueteResult { get; set; }
        public FirmarPaquete firmaPaquete { get; set; }
        public FirmarPaqueteResult firmaPaqueteResult { get; set; }
        public AgregaFirmante agregarFirmante { get; set; }
        public UsuarioDto usuario { get; set; }
        public SupplyOrderDto supplyOrder { get; set; }
        public SOEstimationDto sOEstimation { get; set; }
        public CopadeDto copade { get; set; }
        public AnaliticoPagoDto analiticoPago { get; set; }
        public ReceptionDto reception { get; set; }
        public PaymentScheduleDto paymentSchedule { get; set; }
        public PaymentListDto paymentList { get; set; }
        public Guid usuarioBEId { get; set; }        
        public Guid documentoBEId { get; set; }
        public string Certificado { get; set; }
        public string UrlQR { get; set; }
        public string? hashOriginal { get; set; }
        public string? hashFirma { get; set; }
    }
}
