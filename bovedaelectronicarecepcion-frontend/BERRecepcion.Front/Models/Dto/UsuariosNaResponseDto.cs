using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class UsuariosNaResponseDto
    {
        public Guid UserId { get; set; }
        public string Nacreedor { get; set; }
        public Guid Organismo { get; set; }
        public string Usuario { get; set; }
        public string Compania { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string TipoUsuario { get; set; }
        public Guid ProfileId { get; set; }
        public string Perfil { get; set; }
        //public IEnumerable Rol { get; set; }
        public string RFC { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime ValidoDesde { get; set; }
        public DateTime ValidoHasta { get; set; }
        public DateTime UltimoAcceso { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsDeleted { get; set; }
        public string CreditorRFC { get; set; }
        public int TotalRegistros { get; set; }
    }
}
