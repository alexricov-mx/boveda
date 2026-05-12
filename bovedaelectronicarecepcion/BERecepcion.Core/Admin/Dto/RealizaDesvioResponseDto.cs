using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class RealizaDesvioResponseDto
    {
        public string Contract { get; set; }
        public string SapOrder { get; set; }
        public string Reception { get; set; }
        public string DocumentType { get; set; }
        public string Status { get; set; }
    }
}
