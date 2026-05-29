using System;
using System.Threading.Tasks;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Interfaces.Services.BackEndApi.EstimacionObra;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Services.Uris;

namespace BERRecepcion.Front.Services.BackEndApi.EstimacionObra;

public class EstimacionObra: IEstimacionObra
{
    private readonly IRestUtility _utility;
    private readonly IGenerals _generals;

    public EstimacionObra(IRestUtility utility, IGenerals generals)
    {
        _utility = utility;
        _generals = generals;
    }

    public async Task<PagedResult<SOEstimationDto>> GetPage(int pageNum = 1, int pageSize = 10)
    {
        PagedResult<SOEstimationDto> result = null;
        if (_generals.User.UserType != "UserTypeP")
            result = await _utility.GetItem<PagedResult<SOEstimationDto>>(string.Format(
                UrisEstimacionObra.GetSupplyOrderEstimacionesInterno, _generals.User.Token, pageSize, pageNum));
        else
            result = await _utility.GetItem<PagedResult<SOEstimationDto>>(string.Format(
                UrisEstimacionObra.GetSupplyOrderEstimationProveedor, _generals.User.CreditorNumber, pageSize, pageNum));
        return result;
    }
    
    public async Task<PagedResult<SOEstimationDto>> GetPageByDateRange(DateTime? fechaInicial,
        DateTime? fechaFinal, int pageNum, int pageSize, string search)
    {
        return await _utility.GetItem<PagedResult<SOEstimationDto>>(string.Format(
            UrisEstimacionObra.GetPageByDateRange, search, fechaInicial?.ToString("yyyy-MM-dd"),
            fechaFinal?.ToString("yyyy-MM-dd"),  pageNum, pageSize,_generals.User.UserID, "false"));
    }
}