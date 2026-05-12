using System;

namespace BERRecepcion.Front.Models.Dto
{
    public class DocumentoFirmaBE
    {
        //,string DocumentType, string Orden, Guid usuarioBEId
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
