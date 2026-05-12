using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.eSignDto
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Rfc { get; set; }
        public int Ficha { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Extension { get; set; }
        public bool EsFirmante { get; set; }
        public string estatusdesc { get; set; }
        public int EstatusClave { get; set; }
        public int UsuarioAlta { get; set; }
        public DateTime FechaAlta { get; set; }
        public int UsuarioModif { get; set; }
        public DateTime FechaModif { get; set; }
        public int totalUsuarios { get; set; }
        public virtual UnidadOrgUsuario usuarioRefOrg { get; set; }
    }
}
