using System;
using System.ComponentModel.DataAnnotations;

namespace BERRecepcion.Front.Models.Dto
{
    public class ManagementCentersDto
    {
        public Guid ManagementCenterID { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public string Signer1 { get; set; }
        public string Alternate1 { get; set; }
        public string Signer2 { get; set; }
        public string Alternate2 { get; set; }
        public string Type { get; set; }
        public Guid OrganismID { get; set; }
        public string UsuarioModificador { get; set; }
        public DateTime Fecha { get; set; }
        public virtual string OrganismClave { get; set; }
        public virtual string Signer1Name { get; set; }
        public virtual string Alterate1Name { get; set; }
        public virtual string Signer2Name { get; set; }
        public virtual string Alterate2Name { get; set; }
    }
}
