using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class RealizaDesvioFirmasDto
    {
        public virtual RealizaDesvioItemDto? Contrato { get; set; }
        public virtual IEnumerable<RealizaDesvioItemDto>? OrdenSurtimiento { get; set; }
        public virtual IEnumerable<RealizaDesvioItemDto>? EstimacionObra { get; set; }
        public virtual IEnumerable<RealizaDesvioItemDto>? Recepcion { get; set; }
    }
}
