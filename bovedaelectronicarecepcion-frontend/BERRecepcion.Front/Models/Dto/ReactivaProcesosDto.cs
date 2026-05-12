using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class ReactivaProcesosDto
    {
      
        public String SAPOrderRP { get; set; }

        [Display(Name = "Organismo")]
        public String Name { get; set; }

        [Display(Name = "Numero de Documento")]
        public String TypeDocument { get; set; }
        public String Type { get; set; }
        public String Clave { get; set; }
        [Display(Name = "Contrato")]

        public String Contract { get; set; }
        public String Usuario_Modificador { get; set; }
        public int valida { get; set; }
        public int Firmante { get; set; }
        public OSResponseDto oSResponse { get; set; }

    }
}
