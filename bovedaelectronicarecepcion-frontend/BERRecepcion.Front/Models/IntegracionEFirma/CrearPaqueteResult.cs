using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class CrearPaqueteResult
    {
        public int IdPaquete { get; set; }
        public Guid IdCorrelacion { get; set; }
        public EstatusPaquete Estatus { get; set; }

        public IEnumerable<Firmante> Firmantes { get; set; } = Array.Empty<Firmante>();
        public IEnumerable<Documento> Documentos { get; set; } = Array.Empty<Documento>();

        public class Firmante
        {
            public int IdUsuario { get; set; }
            public string? Figura { get; set; }
        }


        public class Documento
        {
            public Documento()
                : this(0, Guid.Empty, string.Empty, string.Empty, 0, string.Empty, HashAlgorithm.Sha512WithRsa)
            {
            }

            public Documento(int idDocumento, Guid idCorrelacion, string mimeType, string extension, long tamanio, string hash, HashAlgorithm algoritmoHash)
            {
                IdDocumento = idDocumento;
                IdCorrelacion = idCorrelacion;
                MimeType = mimeType;
                Extension = extension;
                Tamanio = tamanio;
                Hash = hash;
                AlgoritmoHash = algoritmoHash;
            }

            public int IdDocumento { get; set; }
            public Guid IdCorrelacion { get; set; }
            public string MimeType { get; set; }
            public string Extension { get; set; }
            public long Tamanio { get; set; }

            public string Hash { get; set; }
            public HashAlgorithm AlgoritmoHash { get; set; }
        }
    }
}