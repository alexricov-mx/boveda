namespace BERecepcion.Core.Copades.Dto;

public class CopadeFiltroRequest
{
    public int PageSize { get; set; }
    public int PageNumber { get; set; } = 1;
    public string? Search { get; set; } = null;
}
