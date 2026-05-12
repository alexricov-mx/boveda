using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class AdmonGRMDto
    {
        public string Contract { get; set; }
        public string? SapOrder { get; set; }
        public string? Reception { get; set; }
        public string CreditorNumber { get; set; }
        public string DocumentType { get; set; }
        public string AdministratorToken { get; set; }
        public DateTime? AdministratorSignDate { get; set; }
    }
}
