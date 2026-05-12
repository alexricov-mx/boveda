using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{
    public class UnidadOrgUsuario
    {
        public int UsuarioId { get; set; }
        public int UnidadId { get; set; }
        public int PerfilId { get; set; }
        public int UsuarioAlta { get; set; }
        public int UsuarioModif { get; set; }
    }
}
