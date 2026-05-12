using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class RecepcionElectronicaPViewModel
    {
        public InvoiceDto invoice { get; set; }

        public ComprobanteBE comprobante { get; set; }

        public IFormFile Archivo { get; set; }

    }
}
