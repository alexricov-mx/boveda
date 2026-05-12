using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.eSignDto
{
    public class UnidadOrgUsuario
    {
        public int UsuarioId { get; set; }
        public int UnidadId { get; set; }
        public int PerfilId { get; set; }
        public int UsuarioAlta { get; set; }
        public int UsuarioModif { get; set; }
        public UnidadOrgUsuario()
        {

        }
        public UnidadOrgUsuario(IConfiguration config)
        {
            UnidadId = int.Parse(config["eSign:UO:UnidadOrganizacionalID"]);
            PerfilId = int.Parse(config["eSign:Usuarios:perfilId:Firmante"]);
            UsuarioAlta = int.Parse(config["eSign:Usuarios:usuarioAltaId"]);
        }
    }
}
