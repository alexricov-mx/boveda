using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.OrdenSurtimiento.Dto
{
    public class SupplyOrderDates
    {
        public DateTime FechaIni { get; set; }

        public DateTime FechaFin { get; set; }

        public SupplyOrderDates(DateTime fechaIni, DateTime fechaFin)
        {
            FechaIni = fechaIni;
            FechaFin = fechaFin;
        }
    }
}
