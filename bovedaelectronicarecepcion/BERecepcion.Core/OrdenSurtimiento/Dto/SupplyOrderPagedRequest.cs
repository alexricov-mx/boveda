namespace BERecepcion.Core.OrdenSurtimiento.Dto;

public class SupplyOrderPagedRequest
{
    public string Token { get; set; }
    public int PageSize { get; set; }
    public string Search { get; set; } = null;
    public int PageNumber { get; set; } = 1;
}
