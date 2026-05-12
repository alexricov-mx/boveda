using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class RealizaDesvioItemDto
    {
        public string Contract { get; set; } 
        public string SapOrder { get; set; }
        public string Reception { get; set; }
        public string DocumentType { get; set; }
        public string Signer { get; set; }
        public string SignerNew { get; set; }
        public string UsuarioModificador { get; set; }
        public string Justificacion { get; set; }
    }
}
