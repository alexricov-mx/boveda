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
    public class CancelacionesController : Controller
    {

        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        protected readonly IConfiguration _configuration;

        public CancelacionesController(IRestUtility utility, IGenerals generals, IConfiguration configuration)
        {
            _utility = utility;
            _generals = generals;
            _configuration = configuration;
        }

        public IActionResult Copade()
        {
            return View("~/Views/Cancelaciones/CancelacionesCopade/Index.cshtml");
        }

        public async Task<IActionResult> BuscarCancelacionesCopade( string busqueda, int pageNum = 1, DateTime? fechaInicial = null, DateTime? fechaFinal = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Bitacora").Value);
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));

                param.Add(new CustomHttpParameter("busqueda", busqueda));
                param.Add(new CustomHttpParameter("token", _generals.User.Token));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                var bitacora = await _utility.GetItem<DataResult<IEnumerable<CancelacionesCopadeDto>>>("Cancelaciones/GetBusquedaCancelacionesCopadeAsync", param);
                
                if (bitacora.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    bitacora.Pager = new Pager(bitacora?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={bitacora.Pager.TotalItems}");
                    bitacora.Pager = new Pager(bitacora.Pager.TotalItems, pageNum, pageSize);
                }
                ViewBag.EmptyResults = bitacora.Pager.TotalItems == 0;
                return PartialView("CancelacionesCopade/_CancelacionesCopadeTable", bitacora);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al Buscar el copade, por favor intente mas tarde" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancelar(CancelacionesCopadeDto dto)
        {
            try
            {

                var dataResult = new DataResult<CancelacionesCopadeDto>
                {
                    Data = dto,
                    User = _generals.User
                };


                dto.user = _generals.User.Token;
                var result = await _utility.Post<DataResult<CancelacionesCopadeDto>>(dataResult, "Cancelaciones/CancelacionesCopadeMasivaAsync");
                if (result.Status == System.Net.HttpStatusCode.OK)
                    return Json(new { Success = true, Message = result.Message });
                else
                    return Json(new { Success = false, Message = result.Message });

            }
            catch (Exception ext)
            {
                return Json(new { Success = false, Message = ext.Message + "Ocurrió un error al Activar/Desactivar las interfases, por favor intente más tarde." });
            }
        }


        #region cancelaciones Programa de pago

        public IActionResult ProgramaPagos()
        {
            return View("~/Views/Cancelaciones/CancelacionesProgramaPago/Index.cshtml");
        }

        public async Task<IActionResult> BuscarCancelacionesProgramaPago(string busqueda, int pageNum = 1, DateTime? fechaInicial = null, DateTime? fechaFinal = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Bitacora").Value);
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("busqueda", busqueda));
                param.Add(new CustomHttpParameter("token", _generals.User.Token));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                var bitacora = await _utility.GetItem<DataResult<IEnumerable<PaymentScheduleDto>>>("Cancelaciones/GetBusquedaCancelacionesPaymentSchedule", param);
                
                if (bitacora.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    bitacora.Pager = new Pager(bitacora?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={bitacora.Pager.TotalItems}");
                    bitacora.Pager = new Pager(bitacora.Pager.TotalItems, pageNum, pageSize);
                }
                ViewBag.EmptyResults = bitacora.Pager.TotalItems == 0;
                return PartialView("~/Views/Cancelaciones/CancelacionesProgramaPago/_CancelacionesProgramaPagoTable.cshtml", bitacora);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al Buscar el Programa de Pago, por favor intente mas tarde" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelarProgramaPago(PaymentScheduleDto dto)
        {
            try
            {

                var dataResult = new DataResult<PaymentScheduleDto>
                {
                    Data = dto,
                    User = _generals.User
                };
              
                dto.TokenTo = _generals.User.Token;
                var result = await _utility.Post<DataResult<PaymentScheduleDto>>(dataResult, "Cancelaciones/CancelacionesPaymentScheduleMasivaAsync");
                
                if (result.Status == System.Net.HttpStatusCode.OK)
                    return Json(new { Success = true, Message = result.Message });
                else
                    return Json(new { Success = false, Message = result.Message });

            }
            catch (Exception ext)
            {
                return Json(new { Success = false, Message = ext.Message + "Ocurrió un error al Cancelar la Programa de Pago, por favor intente mas tarde." });
            }
        }

        #endregion

        #region cancelaciones Lista de pago

        public IActionResult ListaPago()
        {
            return View("~/Views/Cancelaciones/CancelacionesListaPago/Index.cshtml");
        }

        public async Task<IActionResult> BuscarCancelacionesListaPago(string busqueda, int pageNum = 1, DateTime? fechaInicial = null, DateTime? fechaFinal = null)
        {
            try
            {
                fechaInicial = fechaInicial == null ? Convert.ToDateTime(_configuration["infoAplicativo:FechaInicial"]) : fechaInicial;
                fechaFinal = fechaFinal == null ? DateTime.Now : fechaFinal;
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Bitacora").Value);
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.Value.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("busqueda", busqueda));
                param.Add(new CustomHttpParameter("token", _generals.User.Token));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("userId", _generals.User.UserID));
                var bitacora = await _utility.GetItem<DataResult<IEnumerable<PaymentListDto>>>("Cancelaciones/GetBusquedaCancelacionesPaymentListAsync", param);
                
                if (bitacora.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    bitacora.Pager = new Pager(bitacora?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={bitacora.Pager.TotalItems}");
                    bitacora.Pager = new Pager(bitacora.Pager.TotalItems, pageNum, pageSize);
                }
                ViewBag.EmptyResults = bitacora.Pager.TotalItems == 0;
                return PartialView("~/Views/Cancelaciones/CancelacionesListaPago/_CancelacionesListaPagoTable.cshtml", bitacora);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = "Ocurrió un error al Buscar la Lista de Pago, por favor intente mas tarde" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelarListaPago(PaymentListDto dto)
        {
            try
            {

                var dataResult = new DataResult<PaymentListDto>
                {
                    Data = dto,
                    User = _generals.User
                };

                dto.TokenTo = _generals.User.Token;
                var result = await _utility.Post<DataResult<PaymentListDto>>(dataResult, "Cancelaciones/CancelacionesPaymentListMasivaAsync");
                if (result.Status == System.Net.HttpStatusCode.OK)
                    return Json(new { Success = true, Message = result.Message });
                else
                    return Json(new { Success = false, Message = result.Message });

            }
            catch (Exception ext)
            {
                return Json(new { Success = false, Message = ext.Message + "Ocurrió un error al Cancelar la Lista de Pago, por favor intente mas tarde." });
            }
        }

        #endregion

    }
}
