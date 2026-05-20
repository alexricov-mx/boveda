using System.Threading.Tasks;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Models;
namespace BERRecepcion.Front.Services.BackEndApi.OrdenSurtimiento;

public class OrdenSurtimiento
{
    public async Task<PagedResult<SupplyOrderDto>> GetOrdenSurtimiento(string userType,int pageNum = 1, string search = null)
    {
        
        // var ordenSurtimiento = new DataResult<IEnumerable<SupplyOrderDto>>();
        // if (_generals.User.UserType != UserTypeOS)
        // {
        //     param.Add(new CustomHttpParameter(TokenOS, _generals.User.Token));
        //     ordenSurtimiento =
        //         await _utility.GetItem<DataResult<IEnumerable<SupplyOrderDto>>>(
        //             UrisOrdenSurtimiento.GetListOSInterno, param);
        // }
        // else
        // {
        //     param.Add(new CustomHttpParameter(CreditorNumberOS, _generals.User.CreditorNumber));
        //     ordenSurtimiento =
        //         await _utility.GetItem<DataResult<IEnumerable<SupplyOrderDto>>>(
        //             UrisOrdenSurtimiento.GetListOSProveedor, param);
        // }
        return new PagedResult<SupplyOrderDto>();
    }
}