using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Correos.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.eSignDto
{
    public class DocumentoFirmaBE
    {
        public Guid usuarioBEId { get; set; }
        public Guid documentoBEId { get; set; }
        public string DocumentType { get; set; }        
        public BitacoraDto Bitacora { get; set; }
        public CorreoDto Correo { get; set; }
        /*
         * 
         * 
         * Seccion = "Perfiles",
            Accion = "Eliminar",
            Descripcion = string.Concat("Se borra el perfil ", result.Data.Name)
         * 
         */
    }
}
