using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class AddendaViewModel
    {
        public string Property { get; set; }
        public string Value { get; set; }
        public AddendaViewModel(string _property, string _value)
        {
            this.Property = _property;
            this.Value = _value;
        }
    }
}
