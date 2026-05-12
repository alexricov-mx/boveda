using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class UsuariosAuditorPpiDto
    {
        [Required]
        public string usuarioLogeado { get; set; }
        public string? nombreLogeado { get; set; }
        public Guid UserID { get; set; }
        [Required]
        public string userType { get; set; }
        [Required]
        public string userName { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Compania { get; set; }
        [Required]
        public string Email { get; set; }
        public string? status { get; set; }
        public Guid Organismo { get; set; }
        public Guid? Perfil { get; set; }
        public string RFC { get; set; }
        public string CreditorRFC { get; set; }
    }
}
