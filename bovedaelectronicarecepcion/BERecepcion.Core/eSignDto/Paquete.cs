using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.eSignDto
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
        public Paquete()
        {

        }
        public Paquete(IConfiguration config)
        {
            EstatusId = int.Parse(config["eSign:Paquete:estatusId:EnProceso"]);
            UnidadId = int.Parse(config["eSign:UO:UnidadOrganizacionalID"]);
            UsuarioAlta = int.Parse(config["eSign:Usuarios:usuarioAltaId"]);
            Fecha = DateTime.Now;
            FechaAlta = DateTime.Now;
        }
    }
}
