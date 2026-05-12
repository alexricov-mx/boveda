using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class ReactivaProcesosResponseDto
    {
        public Guid ReactivaID { get; set; }
        public Guid OrganismID { get; set; }
        public String Type { get; set; }
        public String DocumentType { get; set; }
        public String Contract { get; set; }
        public String SAPOrderRP { get; set; }
        public Boolean Resultado { get; set; }
        public String Mensaje { get; set; }
        public Boolean Firmante1 { get; set; }
        public String ResultadoTarea1 { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}")]
        [DataType(DataType.DateTime)]
        public DateTime FechaEnvioTarea1 { get; set; }
        public Boolean Firmante2 { get; set; }
        public String ResultadoTarea2 { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}")]
        [DataType(DataType.DateTime)]
        public DateTime FechaEnvioTarea2 { get; set; }
        public String Usuario_Modificador { get; set; }
        public Boolean Activo { get; set; }
    }
}
