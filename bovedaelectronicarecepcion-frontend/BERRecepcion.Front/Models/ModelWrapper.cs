using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class ModelWrapper<T> where T:new()
    {
        public IFormFile file { get; set; }
        //[FromJson]
        public T Model { get; set; }
    }
}
