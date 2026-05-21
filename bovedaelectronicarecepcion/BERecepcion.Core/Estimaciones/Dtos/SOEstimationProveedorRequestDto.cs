namespace BERecepcion.Core.Estimaciones.Dtos;

public class SOEstimationProveedorRequestDto
{

    public string CreditorNumber { get; set; }
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
}
