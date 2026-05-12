using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class DateHelperModel
    {
        public int Order { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Info { get; set; }
        public DateTime? Date { get; set; }
    }
}
