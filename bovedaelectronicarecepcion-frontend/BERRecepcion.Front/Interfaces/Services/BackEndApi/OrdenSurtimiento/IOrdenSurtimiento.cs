using System;
using System.Threading.Tasks;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;

namespace BERRecepcion.Front.Interfaces.Services.BackEndApi.OrdenSurtimiento;

public interface IOrdenSurtimiento
{
    Task<PagedResult<SupplyOrderDto>> GetPage(int pageNum = 1, int pageSize = 10,
        string search = null);

    Task<PagedResult<SupplyOrderDto>> GetPageByDateRange(DateTime? fechaInicial,
        DateTime? fechaFinal, int pageNum, int pageSize, string search);
}