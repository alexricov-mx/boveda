using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class BitacoraDesvioFirmaDto
    {
        public string UsuarioModificador { get; set; }
        public string Signer { get; set; }
        public string SignerNew { get; set; }
        public string Documento { get; set; }
    }
}
