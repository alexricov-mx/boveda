using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.eSignDto
{
    public class DocumentoFirma
    {
        public int PaqueteId { get; set; }
        public int DocumentoId { get; set; }
        public int UsuarioId { get; set; }
        public int AlgoritmoId { get; set; }
        public string AlgoritmoDescripcion { get; set; }
        public string AlgoritmoResult { get; set; }
        public string RepositorioId { get; set; }
        public string TipoCriptografia { get; set; }
        public string InfoFirmado { get; set; }
        public DateTime FechaFirma { get; set; }
        public string CertificadoB64 { get; set; }
        public string CertificadoOcspB64 { get; set; }
        public string CertificadoTspB64 { get; set; }
        public string RespuestaTsp { get; set; }
        public int UsuarioAlta { get; set; }
        public DateTime FechaAlta { get; set; }
        public int UsuarioModif { get; set; }
        public DateTime FechaModif { get; set; }
        public virtual CertificadoDetailDto Certificado { get; set; }
    }

    public class CertificadoDto
    {
        public CertificadoDetailDto Certificado { get; set; }
    }

    public class CertificadoDetailDto
    {
        public string Nombre { get; set; }
        public string NoSerie { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Codificacion { get; set; }
    }
}
