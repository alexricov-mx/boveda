using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{   
    public class estatusPaqueteDto
    {
        public int Id { get; set; }
        public string EstatusDesc { get; set; }
        public int UsuarioModif { get; set; }
    }
}
