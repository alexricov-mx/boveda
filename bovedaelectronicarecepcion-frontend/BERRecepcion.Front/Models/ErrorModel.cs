using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class ErrorModel
    {
        public System.Net.HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
    }
}
