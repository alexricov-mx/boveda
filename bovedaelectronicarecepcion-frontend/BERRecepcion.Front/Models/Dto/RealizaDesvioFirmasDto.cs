using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class RealizaDesvioFirmasDto
    {
        public virtual RealizaDesvioItemDto? Contrato { get; set; }
        public virtual IEnumerable<RealizaDesvioItemDto>? OrdenSurtimiento { get; set; }
        public virtual IEnumerable<RealizaDesvioItemDto>? EstimacionObra { get; set; }
        public virtual IEnumerable<RealizaDesvioItemDto>? Recepcion { get; set; }
    }
}
