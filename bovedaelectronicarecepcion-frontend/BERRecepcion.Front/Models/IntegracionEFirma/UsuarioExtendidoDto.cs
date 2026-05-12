using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class UsuarioExtendidoDto : UsuarioEFirmaDto
    {
        public IEnumerable<UsuarioUnidadOrganizativaDto> UnidadesOrganizativas { get; set; } =
            Array.Empty<UsuarioUnidadOrganizativaDto>();
    }
}
