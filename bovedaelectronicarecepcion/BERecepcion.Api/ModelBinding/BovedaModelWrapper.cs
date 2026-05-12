using BERecepcion.Core.eSignDto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.ModelBinding
{
    public class BovedaModelWrapper
    {
        public IFormFile DocumentoPDF { get; set; }
        [FromJson]
        public ExternosDto Externos { get; set; } // <-- JSON will be deserialized to this object
    }
}
