using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.eSignDto
{
    public class PaqueteFirmante
    {
        public int PaqueteId { get; set; }
        public int UsuarioId { get; set; }
        public int EstatusFirmaId { get; set; }
        public int SuplenteId { get; set; }
        public int OrdenFirmado { get; set; }
        public string Comentarios { get; set; }
        public DateTime FechaRechaza { get; set; }
        public string CertificadoB64 { get; set; }
        public string CertificadoOcspB64 { get; set; }
        public string CertificadoTspB64 { get; set; }
        public string RespuestaTsp { get; set; }
        public int UsuarioAlta { get; set; }
        public DateTime FechaAlta { get; set; }
        public int UsuarioModif { get; set; }
        public DateTime FechaModif { get; set; }
        public PaqueteFirmante()
        {

        }
        public PaqueteFirmante(IConfiguration config)
        {
            EstatusFirmaId = int.Parse(config["eSign:Firma:estatusFirmaId:Pendiente"]);
            OrdenFirmado = 1;
            UsuarioAlta = int.Parse(config["eSign:Usuarios:usuarioAltaId"]);
            FechaAlta = DateTime.Now;
        }
    }
}