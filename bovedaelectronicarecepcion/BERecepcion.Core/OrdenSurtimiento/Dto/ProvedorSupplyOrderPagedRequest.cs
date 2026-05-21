namespace BERecepcion.Core.OrdenSurtimiento.Dto;

public class ProvedorSupplyOrderPagedRequest
{
    public string CreditorNumber { get; set; }
    public int PageSize { get; set; }
    public string Search { get; set; } = null;
    public int PageNumber { get; set; } = 1;
}
