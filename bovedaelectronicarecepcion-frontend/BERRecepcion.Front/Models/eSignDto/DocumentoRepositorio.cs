using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{
    
    public class DocumentoRepositorio
    {
        public int Id { get; set; }
        public int TipoDocId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Mimetype { get; set; }
        public string Hash { get; set; }
        public int algoritmoid { get; set; }
        public string algoritmoresult { get; set; }
        public int Tamanio { get; set; }
        public string RepositorioId { get; set; }
        public int UsuarioAlta { get; set; }
        public int UsuarioModif { get; set; }
        public int PaqueteId { get; set; }
        public int UnidadId { get; set; }
        public IFormFile File { get; set; }        
    }
}
