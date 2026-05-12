using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.FirmaDocumentos.Dto
{
    public class SignersDto
    {
        public DateTime? functionary1signdate { get; set; }
        public DateTime? functionary2signdate { get; set; }
        public string signer1 { get; set; }
        public string signer1Email { get; set; }
        public string alternate1 { get; set; }
        public string alternate1Email { get; set; }
        public string signer2 { get; set; }
        public string signer2Email { get; set; }
        public string alternate2 { get; set; }
        public string alternate2Email { get; set; }
    }
}
