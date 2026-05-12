using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class DesvioPendientesDto
    {
        public virtual DesvioFirmasDto? Contrato { get; set; }
        public virtual IEnumerable<DesvioFirmasDto>? OrdenSurtimiento { get; set; }
        public virtual IEnumerable<DesvioFirmasDto>? EstimacionObra { get; set; }
        public virtual IEnumerable<DesvioFirmasDto>? Recepcion { get; set; }

    }
}
