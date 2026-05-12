using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Facturas.Dto
{
    public class NotasCreditoList
    {
        public List<NotaCredito> NotasCredito { get; set; }
        public NotasCreditoList()
        {
            NotasCredito = new List<NotaCredito>();
        }
    }
}
