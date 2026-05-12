using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Filters;
using RestSharp;
using Microsoft.Extensions.Configuration;

namespace BERRecepcion.Front.Controllers
{

    [ValidateUser]
    public class AdmonGRMController : Controller
    {
        protected readonly IConfiguration _configuration;
        private readonly ILogger<DesvioController> _logger;
        protected readonly IRestUtility _utility;

        public AdmonGRMController(IConfiguration configuration, ILogger<DesvioController> logger, IRestUtility utility)
        {
            _configuration = configuration;
            _logger = logger;
            _utility = utility;
        }

        [RoleFilter(Roles: "GRM")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public ActionResult Index()
        {
            return View();
        }

        // Obtiene registros
        public async Task<IActionResult> GetGRM(string AdministratorToken, int pageNum, int pageSize)
        {
            try
            {                
                if (pageNum == 0)
                    pageNum = 1;
                pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:AdmonGRM").Value); 
                var datos = await _utility.GetItem<DataResult<IEnumerable<AdmonGRMDto>>>("AdmonGRM/"+AdministratorToken+"/"+pageNum+"/"+pageSize);
                var pager = new Pager(datos.Pager.TotalItems, pageNum, pageSize);
                datos.Pager = pager;
                return View("_ConsultaGRM", datos);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }      
    }
}
