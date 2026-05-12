using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class DesvioFirmasDto
    {
        public Guid DesvioID { get; set; }

        [Display(Name = "No. Contrato")]
        public string Contract { get; set; }
        
        [Display(Name = "Orden")]
        public string? SapOrder { get; set; }
        
        [Display(Name = "Recepción")]
        public string? Reception { get; set; }
       
        [Display(Name = "Documento")]
        public string DocumentType { get; set; }

        [Display(Name = "No. Firmante")]
        public string Ficha { get; set; }
        
        [Display(Name = "Firmante")]
        public string Nombre { get; set; }
    }
}
