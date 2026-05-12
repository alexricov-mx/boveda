using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    [AllowAnonymous]
    public class ContratosController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        protected readonly IGenerals _generals;
        public ContratosController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ContratosTable(int pageNum = 1)
        {
            try
            {
                //var param = new List<CustomHttpParameter>();
                //int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:OrdenSurtimiento").Value);
                //param.Add(new CustomHttpParameter("Ficha", _generals.User.Token));
                //param.Add(new CustomHttpParameter("Ficha", _generals.User.Token));
                //param.Add(new CustomHttpParameter("pageSize", pageSize));
                //param.Add(new CustomHttpParameter("pageNum", pageNum));

                //var ordenSurtimiento = await _utility.GetItem<DataResult<IEnumerable<SupplyOrderDto>>>("Firmas/GetOrdenSurtimientoAsync", param);
                //ordenSurtimiento.Pager = new Pager(ordenSurtimiento.Pager.TotalItems, pageNum, pageSize);

                //return PartialView("_OrdenSurtimientoTable", ordenSurtimiento);
                return PartialView("_ContratosTable");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al cargar los contratos, por favor intente mas tarde" });
            }
        }
    }
}
