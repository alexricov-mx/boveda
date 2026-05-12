using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{
    public class Paquete
    {
        public int Id { get; set; }
        public int EstatusId { get; set; }
        public int UnidadId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public int UsuarioAlta { get; set; }
        public DateTime FechaAlta { get; set; }
        public int UsuarioModif { get; set; }
        public DateTime FechaModif { get; set; }
    }
}
