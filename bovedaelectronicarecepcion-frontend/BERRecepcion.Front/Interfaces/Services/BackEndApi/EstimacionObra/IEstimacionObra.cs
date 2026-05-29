using System;
using System.Threading.Tasks;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;

namespace BERRecepcion.Front.Interfaces.Services.BackEndApi.EstimacionObra;

public interface IEstimacionObra
{
    Task<PagedResult<SOEstimationDto>> GetPage(int pageNum = 1, int pageSize = 10);

    Task<PagedResult<SOEstimationDto>> GetPageByDateRange(DateTime? fechaInicial,
        DateTime? fechaFinal, int pageNum, int pageSize, string search);
}