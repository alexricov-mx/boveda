using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{
    public class PaqueteDocumentoDetalleDto
    {
        public int PaqueteId { get; set; }
        public int DocumentoId { get; set; }
        public int OrdenFirmado { get; set; }
        public int TipoDocId { get; set; }
        public string TipoDocDescripcion { get; set; }
        public string Numero { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string MimeType { get; set; }
        public string Hash { get; set; }
        public int AlgoritmoId { get; set; }
        public string AlgoritmoDescripcion { get; set; }
        public string AlgoritmoResult { get; set; }
        public decimal Tamanio { get; set; }
        public string RepositorioId { get; set; }
        public string NombreOrig { get; set; }
    }
}
