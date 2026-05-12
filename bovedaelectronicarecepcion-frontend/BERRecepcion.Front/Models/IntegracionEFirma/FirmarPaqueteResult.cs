using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class FirmarPaqueteResult
    {
        public FirmarPaqueteResult(int idPaquete, EstatusPaquete estatusPaquete, IEnumerable<Documento> documentos)
    {
            IdPaquete = idPaquete;
            EstatusPaquete = estatusPaquete;
            Documentos = documentos;
        }

        public int IdPaquete { get; }
        public EstatusPaquete EstatusPaquete { get; }
        public IEnumerable<Documento> Documentos { get; }

        public class Documento
        {
            public Documento(int idDocumento, Guid idCorrelacion, DateTimeOffset timestampIntegridad, string respuestaTimestampIntegridad)
            {
                IdDocumento = idDocumento;
                IdCorrelacion = idCorrelacion;
                TimestampIntegridad = timestampIntegridad;
                RespuestaTimestampIntegridad = respuestaTimestampIntegridad;
            }

            public int IdDocumento { get; }
            public Guid IdCorrelacion { get; }
            public DateTimeOffset TimestampIntegridad { get; }
            public string RespuestaTimestampIntegridad { get; }
        }
    }
}
