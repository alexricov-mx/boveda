using System;
using System.Threading.Tasks;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;

namespace BERRecepcion.Front.Interfaces.Services.BackEndApi.OrdenBancaria;

public interface IOrdenBancaria
{
    Task<PagedResult<SOEstimationDto>> GetPageByDateRange(DateTime? startDate,
        DateTime? startEnd, int pageNum, int pageSize, string search);
     Task<PagedResult<SOEstimationDto>> GetEstimatesPageByDateRange(DateTime? startDate, DateTime? startEnd,
         string search = null, int pageNum = 1, int pageSize = 10);
 }