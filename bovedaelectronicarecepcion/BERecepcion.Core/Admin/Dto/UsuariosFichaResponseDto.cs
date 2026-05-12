using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class UsuariosFichaResponseDto
    {
        public Guid UserId { get; set; }
        public string Ficha { get; set; }
        public string Usuario { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string TipoUsuario { get; set; }
        public Guid ProfileId { get; set; }
        public string Perfil { get; set; }
        public string Centro { get; set; }
        public string RFC { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime ValidoDesde { get; set; }
        public DateTime ValidoHasta { get; set; }
        public DateTime UltimoAcceso { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsDeleted { get; set; }
        public int TotalRegistros { get; set; }
    }
}
