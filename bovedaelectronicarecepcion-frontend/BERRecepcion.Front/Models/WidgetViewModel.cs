using BERRecepcion.Front.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class WidgetViewModel
    {
        public Guid usuarioBEId { get; set; }
        public Guid documentoBEId { get; set; }
        public string DocumentType { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int UsuarioAlta { get; set; }
        public BitacoraDto Bitacora { get; set; }

    }
}
