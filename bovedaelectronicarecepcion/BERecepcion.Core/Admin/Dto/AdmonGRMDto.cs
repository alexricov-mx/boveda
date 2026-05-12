using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class AdmonGRMDto
    {
        public string Contract { get; set; }
        public string SapOrder { get; set; }
        public string Reception { get; set; }
        public string CreditorNumber { get; set; }
        public string DocumentType { get; set; }
        public string AdministratorToken { get; set; }
        public DateTime? AdministratorSignDate { get; set; }
    }
}
