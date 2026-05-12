using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class BovedaModelWrapper
    {
        public IFormFile DocumentoPDF { get; set; }
        //[FromJson]
        public ExternosDto Externos { get; set; }
    }
}
