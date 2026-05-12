using BERRecepcion.Front.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models
{
    public class SupplyOrderViewModel
    {
        public SupplyOrderDto SupplyOrder { get; set; }
        public List<DateHelperModel> Dates { get; set; }
    }
}
