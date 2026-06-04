using System;

namespace BERecepcion.Core.Consulta.Dto;

public class EstimacionesObraRequest
{
    public int PageSize { get; set; }
    public int PageNumber { get; set; } = 1;
    public DateTime? FechaInicial { get; set; } = null;
    public DateTime? FechaFinal { get; set; } = null;
    public string Search { get; set; } = null;
    public bool EsDescarga { get; set; }  = false;

}
