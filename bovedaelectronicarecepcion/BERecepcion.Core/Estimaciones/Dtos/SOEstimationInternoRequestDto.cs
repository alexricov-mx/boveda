namespace BERecepcion.Core.Estimaciones.Dtos;

public class SOEstimationInternoRequestDto
{
    public string Token { get; set; } = default!;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; }    = 5;
}
