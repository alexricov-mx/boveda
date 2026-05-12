using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    public class InvoiceCxPController : Controller
    {


        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        protected readonly IGenerals _generals;
        public InvoiceCxPController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }

        
        //[RoleFilter(Roles: "")]
        //[UserTypeFilter("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> InvoiceCxPTable(int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:InvoiceCxP").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                var invoiceCxP = new DataResult<IEnumerable<InvoiceCxPList>>();
               
                    invoiceCxP = await _utility.GetItem<DataResult<IEnumerable<InvoiceCxPList>>>("InvoiceCxP/GetInvoiceCxPAsync", param);
              
                    invoiceCxP.Pager = new Pager(invoiceCxP.Pager.TotalItems, pageNum, pageSize);
             
                return PartialView("_InvoiceCxPTable", invoiceCxP);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al cargar la estimación de obra, por favor intente mas tarde" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> InvoiceFiltroCxPTable(string filtro = null, int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:InvoiceCxP").Value);
                param.Add(new CustomHttpParameter("Filtro", filtro));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                var invoiceCxP = new DataResult<IEnumerable<InvoiceCxPList>>();

                invoiceCxP = await _utility.GetItem<DataResult<IEnumerable<InvoiceCxPList>>>("InvoiceCxP/GetInvoiceCxPFiltroAsync", param);

                invoiceCxP.Pager = new Pager(invoiceCxP.Pager.TotalItems, pageNum, pageSize);

                return PartialView("_InvoiceCxPTable", invoiceCxP);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al Buscar, por favor intente mas tarde" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveInvoiceCxP(InvoiceCxPList dto)
        {
            try
            {                   
                // nos traemos el copade
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("Clave", dto.clave));
                param.Add(new CustomHttpParameter("Reception", dto.Reception));
                param.Add(new CustomHttpParameter("Exercise", dto.Exercise));
                var copade = await _utility.GetItem<DataResult<CopadeDto>>("Copade/GetCopadeByReceptionAsync", param);

                var ReqCxP = new DataResult<CXPDto>(){
                    Data = new CXPDto()
                    {
                        Organismo = dto.clave,
                        OrdenSap = dto.SapOrder,
                        Entrada = dto.Reception,
                        Ejercicio = dto.Exercise,
                        FechaRecep = dto.ReceptionDate.ToString("yyyy-MM-dd"),
                        FechaFactura = dto.InvoiceDate.ToString("yyyy-MM-dd"),
                        FechaEmision = dto.CxpSendDate.ToString("yyyy-MM-dd"),
                        Factura = dto.Uuid.ToString(),
                        ViaPago = dto.Assignment,
                        Usuario = _generals.User.Token,
                        ImporteFactura = dto.Total,
                        ImporteOriginal = copade.Data.Total,
                        DiferencialCargo = Convert.ToDouble(dto.Total) < Convert.ToDouble(copade.Data.Total) ? (Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(copade.Data.Total))).ToString() : "0",     // pendiente la regla de diferencial cargo/abono
                        DiferencialAbono = Convert.ToDouble(dto.Total) > Convert.ToDouble(copade.Data.Total) ? (Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(copade.Data.Total))).ToString() : "0",
                    },
                    User = _generals.User
                };

                var invoiceCxP = await _utility.Post<DataResult<CXPDto>>(ReqCxP, "InvoiceCxP/PostInvoiceCxPAsync");
                if(invoiceCxP.Data is null)
                    return Json(new { Success = false, Message = invoiceCxP.Message });

                return Json(new { Success = true, Message = invoiceCxP.Message });
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrio un error al ejecutar la CxP" });
            }
        }

    }
}
