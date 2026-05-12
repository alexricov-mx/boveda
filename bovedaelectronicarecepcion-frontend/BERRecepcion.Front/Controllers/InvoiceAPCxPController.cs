using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
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
    public class InvoiceAPCxPController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        protected readonly IGenerals _generals;
        public InvoiceAPCxPController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
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
        public async Task<IActionResult> InvoiceAPCxPTable(int pageNum = 1)
        {
            try
            {
                
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:InvoiceAPCxP").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                var invoiceAPCxP = new DataResult<IEnumerable<InvoiceAPCxPListDto>>();

                invoiceAPCxP = await _utility.GetItem<DataResult<IEnumerable<InvoiceAPCxPListDto>>>("InvoiceAPCxP/GetInvoiceAPCxPAsync", param);

                invoiceAPCxP.Pager = new Pager(invoiceAPCxP.Pager.TotalItems, pageNum, pageSize);

                return PartialView("_InvoiceAPCxPTable", invoiceAPCxP);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al cargar datos de Invoice AP CxP, por favor intente mas tarde" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> InvoiceFiltroAPCxPTable(string filtro = null, int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:InvoiceAPCxP").Value);
                param.Add(new CustomHttpParameter("Filtro", filtro));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));

                var invoiceCxP = new DataResult<IEnumerable<InvoiceCxPList>>();

                invoiceCxP = await _utility.GetItem<DataResult<IEnumerable<InvoiceCxPList>>>("InvoiceCxP/GetInvoiceCxPFiltroAsync", param);

                invoiceCxP.Pager = new Pager(invoiceCxP.Pager.TotalItems, pageNum, pageSize);

                return PartialView("_InvoiceAPCxPTable", invoiceCxP);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al Buscar, por favor intente mas tarde" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> SaveInvoiceAPCxP(InvoiceAPCxPListDto dto)
        {
            try
            {

                var analitico = await _utility.GetItem<DataResult<AnaliticoPagoDto>>($"AnaliticoPago/GetAPByIdAnaliticoAsync/{dto.idAnalitico}");

                var ReqCxP = new DataResult<CXPDto>()
                {
                    Data = new CXPDto()
                    {
                        Organismo = dto.clave,                        
                        Ejercicio = dto.Ejercicio,
                        FechaRecep = dto.ReceptionDate.ToString("yyyy-MM-dd"),
                        FechaFactura = dto.InvoiceDate.ToString("yyyy-MM-dd"),                        
                        Factura = dto.Uuid == Guid.Empty ? string.Concat(dto.Serie, " ", dto.Folio) : dto.Uuid.ToString(),
                        ContratoVigente = analitico.Data.Contrato,
                        Cliente = analitico.Data.NumCliente,
                        Id_Analitico = analitico.Data.IdAnalitico,
                        CentroGestor = analitico.Data.Centro,
                        ViaPago = dto.Assignment,
                        Usuario = _generals.User.Token,
                        ImporteFactura = dto.Total,
                        ImporteOriginal = analitico.Data.Total,
                        DiferencialCargo = Convert.ToDouble(dto.Total) < Convert.ToDouble(analitico.Data.Total) ? (Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(analitico.Data.Total))).ToString() : "0",     // pendiente la regla de diferencial cargo/abono
                        DiferencialAbono = Convert.ToDouble(dto.Total) > Convert.ToDouble(analitico.Data.Total) ? (Math.Abs(Convert.ToDouble(dto.Total) - Convert.ToDouble(analitico.Data.Total))).ToString() : "0",
                    },
                    User = _generals.User
                };

                var invoiceCxP = await _utility.Post<DataResult<CXPDto>>(ReqCxP, "InvoiceAPCxP/PostInvoiceCxPAPAsync");
                return Json(new { Success = true, Message = invoiceCxP.Message });
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "Ocurrio un error al ejecutar la AP CxP" });
            }
        }


    }
}
