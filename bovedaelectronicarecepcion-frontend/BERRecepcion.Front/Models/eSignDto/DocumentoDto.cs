using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{    
    public class DocumentoDto
    {
        public int Id { get; set; }
        public int TipoDocId { get; set; }
        public string Numero { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string Mimetype { get; set; }
        public string Hash { get; set; }
        public int algoritmoid { get; set; }
        public string algoritmoresult { get; set; }
        public int Tamanio { get; set; }
        public string RepositorioId { get; set; }
        public int UsuarioAlta { get; set; }
        public DateTime FechaAlta { get; set; }
        public int UsuarioModif { get; set; }
        public DateTime FechaModif { get; set; }
        public int PaqueteId { get; set; }
        public string NombreOrig { get; set; }
    }
}
