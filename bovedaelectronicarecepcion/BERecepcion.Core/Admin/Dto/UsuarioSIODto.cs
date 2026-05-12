using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class UsuarioSIODto
    {
        public UsuarioSIODtoItem FichaResult { get; set; }
    }

    public class UsuarioSIODtoItem
    {
        
        public string NOMBRES { get; set; }
        public string AP_PATERNO { get; set; }
        public string AP_MATERNO { get; set; }
        public string EMAIL { get; set; }
        private string CLAVE;
        public string RFC_SAT { get; set; }
        public string DEPTO_CLAVE { get; set; }
        public string FICHA { get; set; }
        public string ORG_CLAVE
        {
            get { return CLAVE; }
            set
            {
                CLAVE = OrganismoMapea(value);
            }
        }

        public static string OrganismoMapea(string value)
        {
            string CLAVE = "";
            switch (value)
            {
                case "4":
                    CLAVE = "CORP";
                    break;
                case "7":
                    CLAVE = "PEP";
                    break;
                case "8":
                    CLAVE = "REF";
                    break;
                case "9":
                    CLAVE = "PGPB";
                    break;
                case "10":
                    CLAVE = "PPQ";
                    break;
                case "30":
                    CLAVE = "PTRI";
                    break;
                case "31":
                    CLAVE = "PPER";
                    break;
                case "32":
                    CLAVE = "PLOG";
                    break;
                case "33":
                    CLAVE = "PCOG";
                    break;
                case "34":
                    CLAVE = "PFER";
                    break;
                case "35":
                    CLAVE = "PETI";
                    break;
                default:
                    CLAVE = "XX";
                    break;
            }
            return CLAVE;
        }
    }
}
