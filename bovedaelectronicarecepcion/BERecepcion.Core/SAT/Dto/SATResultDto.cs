using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.SAT.Dto
{
    public class SATResultDto
    {
        public List<ValidationError> validationErrors { get; set; }
        public bool existsCriticalError { get; set; }
    }
}
