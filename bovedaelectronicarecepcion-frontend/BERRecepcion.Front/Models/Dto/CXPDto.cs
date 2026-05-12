using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Models.Dto
{
    public class CXPDto
    {
        public string Organismo { get; set; }
        public string OrdenSap { get; set; }
        public string Entrada { get; set; }
        public string? Ejercicio { get; set; }
        public string FechaRecep { get; set; }
        public string FechaFactura { get; set; }
        public string? FechaEmision { get; set; }
        public string Factura { get; set; }
        public string? ViaPago { get; set; }
        public string? Usuario { get; set; }
        public string? Res { get; set; }
        public string? ContratoVigente { get; set; }
        public string? Cliente { get; set; }
        public string? Id_Analitico { get; set; }
        public string? CentroGestor { get; set; }
        public string ImporteFactura { get; set; }
        public string ImporteOriginal { get; set; }
        public string DiferencialCargo { get; set; }
        public string DiferencialAbono { get; set; }
        public string? DocumentoSAP { get; set; }
        public string? Mensaje { get; set; }
    }
}