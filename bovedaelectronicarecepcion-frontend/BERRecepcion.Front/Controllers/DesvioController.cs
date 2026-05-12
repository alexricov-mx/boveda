using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using BERRecepcion.Front.Filters;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class DesvioController : Controller
    {
        private readonly ILogger<DesvioController> _logger;
        private readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public DesvioController(ILogger<DesvioController> logger, IRestUtility utility, IGenerals generals)
        {
            _logger = logger;
            _utility = utility;
            _generals = generals;
        }

        [RoleFilter(Roles: "DeviationSigns")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public IActionResult Index()
        {

            return View();
        }

        // Obtiene documentos.
        [HttpGet]
        public async Task<IActionResult> Documentos(string contrato)
        {
            try
            {
                contrato = contrato == null ? "0000000000" : contrato;
                var datos = await _utility.GetItem<DataResult<DesvioPendientesDto>>("DesvioFirmas/pendientes/" + contrato);
                if(datos.Data == null)
                    return Json(new { success = false, message = "Ocurrió un error al consultar el contrato, por facor intente  más tarde." });
                if (datos.Data.Contrato == null && datos.Data.EstimacionObra.Count() == 0 && datos.Data.OrdenSurtimiento.Count() == 0 && datos.Data.Recepcion.Count() == 0)
                    return Json(new { success = false , message = "No se encontró información del contrato."});
                return PartialView("_Documentos", datos.Data);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Ocurrió un error al consultar el contrato, por facor intente más tarde." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFirmante(string Token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Token))
                    return Json(new {success = false, message ="Debe ingresar un número de ficha" });
                
                var datos = await _utility.GetItem<DataResult<UsuariosFichaResponseDto>>("Usuarios/Simple/" + Token);

                if (datos.Data == null)
                    return Json(new { success = false, message = $"El usuario con la ficha {Token} no existe, favor de validar." });

                return Json(new { success = true, datos.Data });
            }
            catch (Exception)
            {
                throw;
            }
        }
        // Obtiene firmante actual.
        //[HttpGet]
        //public async Task<IActionResult> GetFirmante(string Token)
        //{
        //    try
        //    {
        //        if (Token == null)
        //        {
        //            Token = "000000";
        //        }
        //        var datos = await _utility.GetItem<DataResult<UsuariosFichaResponseDto>>("Usuarios/Simple/" + Token);
        //        var ficha = "";
        //        var nombre = "";
        //        if (datos.Data == null)
        //        {
        //            ficha = "";
        //            nombre = "¡El firmante con la ficha "+ Token + " no existe!";                   
        //        }
        //        else{
        //            ficha = datos.Data.Ficha.ToString();
        //            nombre = datos.Data.Nombre.ToString();
        //        }
        //        ViewBag.ficha = ficha;
        //        ViewBag.nombre = nombre;
        //        return PartialView("_Firmante", datos.Data);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        // Genera el desvio de firma.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(RealizaDesvioFirmasDto desvio)
        {
            //var userData = User.Identities.Where(x => x.AuthenticationType.Equals("UserData")).FirstOrDefault();
            //var item = userData.Claims.Where(x => x.Type.Equals("Ficha")).FirstOrDefault().Value;

            var item = _generals.User.Token;

            //var item = "111111";

            if (desvio.Contrato != null)
            {
                desvio.Contrato.UsuarioModificador = item;
            }

            if (desvio.OrdenSurtimiento != null)
            {
                foreach (var i in desvio.OrdenSurtimiento)
                {
                    i.UsuarioModificador = item;
                }
            }

            if (desvio.EstimacionObra != null)
            {
                foreach (var i in desvio.EstimacionObra)
                {
                    i.UsuarioModificador = item;
                }
            }

            if (desvio.Recepcion != null)
            {
                foreach (var i in desvio.Recepcion)
                {
                    i.UsuarioModificador = item;
                }
            }

            try
            {
                var datos = await _utility.Post<RealizaDesvioFirmasDto>(desvio, "DesvioFirmas");
                
                if(datos != null) 
                    return Json(new { success = true, message = "Desvío de firma realizado correctamente." });
                return Json(new { success = false, message = "Ha ocurrido un problema, por favor intenta nuevamente." });
            }
            catch (Exception exp)
            {
                return Json(new { success = false, message = "Ha ocurrido un problema, por favor intenta nuevamente." });
            }
        }
    }
}
