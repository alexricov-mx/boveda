using System;
using System.Collections.Generic;
using System.Text;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Dto;

namespace BERecepcion.Core.Facturas.Dto
{
    public class ComprobanteDto
    {
        public string rutaArchivo { get; set; }
        public string claveOrganismo { get; set; }
        public ComprobanteBE comprobante { get; set; }
        public IEnumerable<ComprobanteBE> notasCredito { get; set; }
        public IEnumerable<NotaCreditoCFDIXMLDto> notasCreditoCFDIXML { get; set; }
        public IEnumerable<ValidationError> validationErrors { get; set; }     //lista de errores
        public CopadeDto copade { get; set; }      //copade que ampara
        public int idLastStep { get; set; }     //bitacora
        public int status { get; set; }         // 1- aceptada -1 - rechazada 0 - en proceso
        public DateTime? fechaRecepcion { get; set; }
        public DateTime? fechaRechazo { get; set; }
        public DateTime? fechaContabilizacion { get; set; }
        public DateTime fechaUltimaModificacion { get; set; }
        public bool esLote { get; set; }        //si fue procesada individual o por lote
        public bool esDocumental { get; set; }      //es documental o electronica
        public bool esCopade { get; set; }
        public string OriginalXml { get; set; }
        public ComprobanteDto()
        {
            notasCredito = new List<ComprobanteBE>();
            validationErrors = new List<ValidationError>();
            copade = new CopadeDto();
        }
        public IEnumerable<string> result { get; set; }
        public bool existedException { get; set; }
        public string exceptionMessage { get; set; }
        public UsersDto User { get; set; }

        public string ComprobanteBEString { get; set; }
        public string ComprobanteOriginal { get; set; }
        public string CFDIVersion { get; set; }
    }
}