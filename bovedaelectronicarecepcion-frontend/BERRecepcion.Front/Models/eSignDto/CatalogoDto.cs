using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{
    public class CatalogoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Siglas { get; set; }
        public string Descripcion { get; set; }
        public int Orden { get; set; }
        public string DescripcionLimpia { get; set; }
        public int? DependeId { get; set; }
        public int UsuarioAlta { get; set; }
        public DateTime FechaAlta { get; set; }
        public int UsuarioModif { get; set; }
        public DateTime FechaModif { get; set; }
    }
}
