using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Interfaces.Services.BackEndApi.OrdenBancaria;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BERRecepcion.Front.Controllers;

[ValidateUser]
public class OrdenBancariaController : Controller
{
    protected readonly IConfiguration _configuration;
    protected readonly IGenerals _generals;
    private readonly IOrdenBancaria _ordenBancaria;

    public OrdenBancariaController(IConfiguration configuration, IGenerals generals,
        IOrdenBancaria ordenBancaria)
    {
        _configuration = configuration;
        _generals = generals;
        _ordenBancaria = ordenBancaria;
    }

    #region Refactor

    [HttpGet]
    public async Task<IActionResult> GetPageByDateRange(DateTime? startDate, DateTime? endDate, int pageNum = 1,
        IEnumerable<string> search = null)
    {
        int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
        startDate = startDate == null
            ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"])
            : startDate;
        endDate = endDate == null ? DateTime.Now : endDate;

        return PartialView("~/Views/Consultas/OrdenesBancarias/GenerarTablaOrdenSurtimiento.cshtml",
            await _ordenBancaria.GetPageByDateRange(startDate, endDate, pageNum, pageSize,
                _generals.GetStringSearch(search)));
    }

    [HttpGet]
    public async Task<IActionResult> GetEstimacionesBancariasTable(DateTime? startDate, DateTime? endDate,
        int pageNum = 1, IEnumerable<string> search = null)
    {
        int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
        startDate = startDate == null
            ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"])
            : startDate;
        endDate = endDate == null ? DateTime.Now : endDate;

        return PartialView("~/Views/Consultas/EstimacionesBancarias/GenerarTablaEstimacion.cshtml",
            await _ordenBancaria.GetEstimatesPageByDateRange(startDate, endDate, _generals.GetStringSearch(search),
                pageNum, pageSize));
    }

    #endregion refactor
}