using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class DesvioFirmasDto
    {
        public string Contract { get; set; }
#nullable enable
        public string? SapOrder { get; set; }
        public string? Reception { get; set; }
#nullable disable
        public string DocumentType { get; set; }
        public string Ficha { get; set; }
        public string Nombre { get; set; }
        public string Peso { get; set; }
    }
}
