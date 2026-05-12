using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{
	public class OrganismDto
	{
        public Guid OrganismID { get; set; }
        public Guid usuarioId { get; set; }
        public string Name { get; set; }
        public string Clave { get; set; }
        public string Address { get; set; }
        public string Rfc { get; set; }
        //, uo.Company, uo.CreditorNumber, uo.CreditorRFC, uo.EmailAlternate
        public string? Company { get; set; }
        public string? CreditorNumber { get; set; }
        public string? CreditorRFC { get; set; }
        public string? EmailAlternate { get; set; }
    }
}
