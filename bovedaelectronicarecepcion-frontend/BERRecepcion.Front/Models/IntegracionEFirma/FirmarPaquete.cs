using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class FirmarPaquete
    {
        public FirmarPaquete() : this(
        null,
        null,
        0,
        null,
        string.Empty,
        null,
        Array.Empty<Firma>()
        )
        {
        }

        public FirmarPaquete(
            int? idPaquete,
            Guid? idCorrelacion,
            int idUsuarioFirmante,
            string? figuraFirmante,
            string certificadoFirmanteBase64,
            string? nombreArchivoCertificadoFirmante,
            IEnumerable<Firma> firmas
            )
        {
                IdPaquete = idPaquete;
                IdCorrelacion = idCorrelacion;
                IdUsuarioFirmante = idUsuarioFirmante;
                FiguraFirmante = figuraFirmante;
                CertificadoFirmanteBase64 = certificadoFirmanteBase64;
                NombreArchivoCertificadoFirmante = nombreArchivoCertificadoFirmante;
                Firmas = firmas;
        }

        public int? IdPaquete { get; set; }
        public Guid? IdCorrelacion { get; set; }
        public int IdUsuarioFirmante { get; set; }
        public string? FiguraFirmante { get; set; }
        public string CertificadoFirmanteBase64 { get; set; }
        public string? NombreArchivoCertificadoFirmante { get; set; }
        public IEnumerable<Firma> Firmas { get; set; }

    }
}
