using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
	public class BitacoraDto
	{
		public long Secuencia { get; set; }
		public Guid? UserID { get; set; }
		public string Seccion { get; set; }
		public string Accion { get; set; }
		public string Descripcion { get; set; }
		public DateTime Fecha { get; set; }

        public virtual string UserName { get; set; }
        public BitacoraDto()
        {
        }
        public BitacoraDto(Guid _userID, string _seccion, string _accion, string _descripcion)
        {
			this.UserID = _userID;
			this.Seccion = _seccion;
			this.Accion = _accion;
			this.Descripcion = _descripcion;
        }
    }
}
