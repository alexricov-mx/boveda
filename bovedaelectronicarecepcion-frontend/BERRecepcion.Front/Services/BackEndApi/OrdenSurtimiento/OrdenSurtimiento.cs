using System.Threading.Tasks;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Interfaces.Services.BackEndApi.OrdenSurtimiento;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Services.Uris;
namespace BERRecepcion.Front.Services.BackEndApi.OrdenSurtimiento;

public class OrdenSurtimiento: IOrdenSurtimiento
{
    private readonly IRestUtility _utility;
    private readonly IGenerals _generals;

    public OrdenSurtimiento(IRestUtility utility, IGenerals generals)
    {
        _utility = utility;
        _generals = generals;
    }
    
    public async Task<PagedResult<SupplyOrderDto>> GetPage(int pageNum = 1, int pageSize = 10,  string search = null)
    {
        if (_generals.User.UserType != "UserTypeP")
            return await _utility.GetItem<PagedResult<SupplyOrderDto>>(string.Format(UrisOrdenSurtimiento.GetListOSInternoRefactor, _generals.User.Token, pageSize, pageNum, search));
        else
            return await _utility.GetItem<PagedResult<SupplyOrderDto>>(string.Format(UrisOrdenSurtimiento.GetListOSProveedorRefactor, _generals.User.CreditorNumber, pageSize, pageNum, search));
    }
}