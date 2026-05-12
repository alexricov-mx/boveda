using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class UsuariosPemexInDto
    {
#nullable enable
        public Guid? userId { get; set; }
        public string? usuarioLogeado { get; set; }
        public string? nombreLogeado { get; set; }
        public string? Name { get; set; }        
        public string? Ficha { get; set; }
        public string? Centro { get; set; }
        public string? RFC { get; set; }        
        public Guid? Perfil { get; set; }        
        public string? userName { get; set; }
        public string? Email { get; set; }        
        public string? status { get; set; }       
        public string? CreditorNumber { get; set; }
        public string? PhoneNumber { get; set; }
#nullable disable
        public string CreditorRFC { get; set; }
        public string userType { get; set; }
        public string Organismo { get; set; }
        public List<OrganismDto>? Organismos { get; set; }
        public string ValidoDesde { get; set; }
        public string ValidoHasta { get; set; }
        public string Compania { get; set; }

        public byte[]? FileDI { get; set; }
        public string NameFileDI { get; set; }
        public byte[]? FileDP { get; set; }
        public string NameFileDP { get; set; }

    }
}
