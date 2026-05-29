using System;
using System.Linq;
using System.Threading.Tasks;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Interfaces.Services.BackEndApi.OrdenBancaria;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Services.Uris;

namespace BERRecepcion.Front.Services.BackEndApi.OrdenBancaria;

public class OrdenBancaria : IOrdenBancaria
{
    private readonly IRestUtility _utility;
    private readonly IGenerals _generals;

    public OrdenBancaria(IRestUtility utility, IGenerals generals)
    {
        _utility = utility;
        _generals = generals;
    }

    public async Task<PagedResult<SOEstimationDto>> GetPageByDateRange(DateTime? startDate,
        DateTime? startEnd, int pageNum, int pageSize, string search)
    {
        var org = _generals.User.Organisms.FirstOrDefault();
        return await _utility.GetItem<PagedResult<SOEstimationDto>>(string.Format(
            UrisOrdenBancaria.GetPageByDateRange, search, startDate?.ToString("yyyy-MM-dd"),
            startEnd?.ToString("yyyy-MM-dd"), pageNum, pageSize, _generals.User.UserID, (org != null ? org.Clave : ""),
            "", "false"));
    }

    public async Task<PagedResult<SOEstimationDto>> GetEstimatesPageByDateRange(DateTime? startDate, DateTime? endDate,
        string search, int pageNum, int pageSize)
    {
        return await _utility.GetItem<PagedResult<SOEstimationDto>>(string.Format(
            UrisOrdenBancaria.GetEstimatePageByDateRange, search, startDate?.ToString("yyyy-MM-dd"),
            endDate?.ToString("yyyy-MM-dd"), pageNum, pageSize,
            _generals.User.UserID, "false"));
    }
}