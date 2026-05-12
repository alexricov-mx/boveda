using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    //[AllowAnonymous]
    public class ReactivaProcesosController : Controller
    {
        private readonly ILogger<ReactivaProcesosController> _logger;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public ReactivaProcesosController(ILogger<ReactivaProcesosController> logger, IRestUtility utility, IGenerals generals)
        {
            _logger = logger;
            _utility = utility;
            _generals = generals;
        }
        [RoleFilter(Roles: "AdministrationValidations")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public IActionResult Index()
        {
           return View();
        }

        public async Task<IActionResult> BuscaReactivarProcesos(string SAPOrder)
        {

            try
            {
                var datos = await _utility.GetItem<DataResult<ReactivaProcesosDto>>("ReactivaProcesos/" + SAPOrder);              
                var datos2 = await _utility.GetItem<DataResult<IEnumerable<ReactivaProcesosResponseDto>>>("ReactivaProcesos/GetReactivaProcesos/" + SAPOrder);

                var datosUnio = new UnionReactivarProcesosDto()
                {

                    ReactivaProcesosFirma = datos.Data,
                    ReactivaProcesosDatos = datos2.Data
                };               
                
                return PartialView("_RPFirmas", datosUnio);
            }
            catch (Exception ext)
            {
                return Json(new { success = false, responseText = ext.Message });
            }
        
        }

        public async Task<JsonResult> FirmarReactivarProcesos(ReactivaProcesosDto reactP)
        {
            try
            {
                reactP.Usuario_Modificador = _generals.User.Token == null ? "" : _generals.User.Token;
                
                OSResponseItemDto osItem = new OSResponseItemDto() { 
                    CONTRATO = reactP.Contract,
                    ORDEN_SAP = reactP.SAPOrderRP,
                    ORGANISMO = reactP.Clave,
                    TIPO = reactP.Type
                };

                OSResponseDto osResponse = new OSResponseDto() { item = osItem };

                reactP.oSResponse = osResponse;

                DataResult<ReactivaProcesosDto> data = new DataResult<ReactivaProcesosDto>()
                {
                    Data = reactP
                };
                
                var datos = await _utility.Post<DataResult<ReactivaProcesosDto>>(data, "ReactivaProcesos");

                if (datos.Data == null)
                {
                    return Json(new { success = false, responseText = "Error en el envio de informacion" });
                }
                else
                {
                    return Json(new { success = true, responseText = "Envio de informacion con exito" });
                }
            }
            catch (Exception exp)
            {
                
                return Json(new { success = false, responseText = exp.Message });
            }

        }


        
    }
}
