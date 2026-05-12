using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class PICopadeRequestDto
    {
        public string Clave { get; set; }
        public string SapOrder { get; set; }
        public string Reception { get; set; }
        public string Exercise { get; set; }
        public bool Functionary1SignDate { get; set; }
        public Guid CopadeId { get; set; }
    }

    public class DtoPdfPreviewData
    {
        public PICopadeRequestDto data { get; set; }
        public IEnumerable<string> result { get; set; }
        public string UsuarioModificador { get; set; }
    }

}
