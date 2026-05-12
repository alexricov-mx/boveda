using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class UsuarioUnidadOrganizativaDto : DataTransferObject
    {
        public int IdUsuario { get; set; }
        public int IdUnidadOrganizativa { get; set; }
        public PerfilUsuario Perfil { get; set; }
        public EstatusUsuario Estatus { get; set; }
    }
}
