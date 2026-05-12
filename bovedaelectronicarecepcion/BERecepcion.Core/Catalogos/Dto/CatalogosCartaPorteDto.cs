using System;
using System.Data;

namespace BERecepcion.Core.Catalogos.Dto
{
    public class CatalogosCartaPorteDto
    {
        public bool? isValid { get; set; }
        public byte[]? ArchivoXLS { get; set; }
        public DataSet? CatalogosXLS { get; set; }

        public string? tableName { get; set; }

        public string? value { get; set; }
        public DateTime? InicioVigencia { get; set; }
        public DateTime? FinVigencia { get; set; }

        public string? c_Colonia { get; set; }
        public string? c_CodigoPostal { get; set; }
        public string? NombreAsentamiento { get; set; }

        public string? c_Localidad { get; set; }
        public string? c_Estado { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaInicioVigencia { get; set; }
        public DateTime? FechaFinVigencia { get; set; }

        public string? c_Municipio { get; set; }

        public string? Name { get; set; }
        public DateTime? Fecha { get; set; }
    }
}
