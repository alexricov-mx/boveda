using System.Collections.Generic;
using System.Threading.Tasks;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Interfaces.Services.BackEndApi.Procesos;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Services.Uris;

namespace BERRecepcion.Front.Services.BackEndApi.Procesos;

public class Procesos : IProcesos
{
    private readonly IRestUtility _utility;
    private readonly IGenerals _generals;

    public Procesos(IRestUtility utility, IGenerals generals)
    {
        _utility = utility;
        _generals = generals;
    }

    public async Task<IEnumerable<ReactivaProcesosResponseDto>> GetReactivateProcessAsync(string SAPOrder)
    {
        return await _utility.GetItem<IEnumerable<ReactivaProcesosResponseDto>>(string.Format(
            UrisProcesos.GetReactivateProcessAsync, _generals.User.Token, SAPOrder));
    }
    public async Task<ReactivaProcesosDto> GetSAPSignatureDeliveryAsync(string SAPOrder)
    {
        return await _utility.GetItem<ReactivaProcesosDto>(string.Format(
            UrisProcesos.GetSAPSignatureDeliveryAsync, _generals.User.Token, SAPOrder));
    }
}