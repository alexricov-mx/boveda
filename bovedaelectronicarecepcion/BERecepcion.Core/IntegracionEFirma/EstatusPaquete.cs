using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.IntegracionEFirma
{
    public enum EstatusPaquete
    {
        Borrador = 1,
        EnProcesoFirma,
        Rechazado,
        Firmado,
        Cancelado,
        Eliminado
    }
}