using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Interfaces.Services.BackEndApi.OrdenSurtimiento;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Services.Uris;

namespace BERRecepcion.Front.Services.BackEndApi.OrdenSurtimiento;

public class OrdenSurtimiento : IOrdenSurtimiento
{
    private readonly IRestUtility _utility;
    private readonly IGenerals _generals;

    public OrdenSurtimiento(IRestUtility utility, IGenerals generals)
    {
        _utility = utility;
        _generals = generals;
    }

    public async Task<PagedResult<SupplyOrderDto>> GetPage(int pageNum = 1, int pageSize = 10, string search = null)
    {
        if (_generals.User.UserType != "UserTypeP")
            return await _utility.GetItem<PagedResult<SupplyOrderDto>>(string.Format(
                UrisOrdenSurtimiento.GetSupplyOrderInternoAsync, _generals.User.Token, pageNum, pageSize,
                search ?? ""));
        else
            return await _utility.GetItem<PagedResult<SupplyOrderDto>>(string.Format(
                UrisOrdenSurtimiento.GetSupplyOrderProveedorAsync, _generals.User.CreditorNumber, pageNum, pageSize,
                search ?? ""));
    }

    public async Task<PagedResult<SupplyOrderDto>> GetPageByDateRange(DateTime? fechaInicial,
        DateTime? fechaFinal, int pageNum, int pageSize, string search)
    {
        return await _utility.GetItem<PagedResult<SupplyOrderDto>>(string.Format(
            UrisOrdenSurtimiento.GetPageByDateRange, _generals.User.UserID, fechaInicial?.ToString("yyyy-MM-dd"),
            fechaFinal?.ToString("yyyy-MM-dd"), search, pageNum, pageSize, "false"));
    }

    //public async Task<string> GetExpedient(string orderSAP)
    //{
        
    //}
}