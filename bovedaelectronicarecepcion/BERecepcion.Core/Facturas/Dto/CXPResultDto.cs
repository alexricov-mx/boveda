using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class CXPResultDto
    {
        public bool existsCriticalError { get; set; }
        public List<ValidationError> validationErrors { get; set; }
        public string sapError { get; set; }
        public string sapDocument { get; set; }
    }
}
