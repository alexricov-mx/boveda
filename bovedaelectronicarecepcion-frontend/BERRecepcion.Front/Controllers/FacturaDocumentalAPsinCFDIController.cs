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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BERRecepcion.Front.Controllers
{
    [AllowAnonymous]
    public class FacturaDocumentalAPsinCFDIController : Controller
    {
        private readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public FacturaDocumentalAPsinCFDIController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        public async Task<IActionResult> Index()
        {
            return await Task.Run(() => View(_generals.User.Organisms.GroupBy(x => x.Name).Select(group => group.First())));
        }

        [HttpPost]
        public async Task<IActionResult> SaveInvoice(string Organismo, string IdAnaliticoPago, string RFCReceptor, string Correo, InvoiceDto dto)
        {
            try
            {
                string resultMessage = "";
                bool success = false;
                bool criticalError = false;

                InvoiceResultDto _dto = new InvoiceResultDto();
                _dto.Organismo = Organismo;
                _dto.IdDocumento = IdAnaliticoPago;
                _dto.RFCReceptor = RFCReceptor;
                _dto.InvoiceDto = dto;
                _dto.User = _generals.User;
                _dto.Correo = Correo;
                
                var result = await _utility.Post(_dto, "FacturaDocumental/SaveInvoice");

                if(result!=null)
                {
                    if(result.ExistedException)
                    {
                        resultMessage = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                        success = false;
                        criticalError = true;
                    }
                    else
                    {
                        List<ValidationError> validationErrors = result.validationErrors.ToList();
                        if(validationErrors.Count==0)
                        {
                            resultMessage = "La informacion fue procesada correctamente.";
                            success = true;
                        }
                        else
                        {
                            var cxperror = validationErrors.Where(x => x.clave == "60000" || x.clave == "60001").FirstOrDefault();
                            if (cxperror != null)
                            {
                                resultMessage = cxperror.mensaje;
                                success = true;
                            }
                            else
                            {
                                success = false;
                                return Json(new { Success = success, CriticalError = criticalError, Message = resultMessage, ValidationErrors = validationErrors });
                            }
                        }
                    }
                }
                else
                {
                    resultMessage = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                    success = false;
                    criticalError = true;
                }

                return Json(new { Success = success, CriticalError = criticalError, Message = resultMessage });
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, CriticalError=true, isTimeOut = true, Message = "Continuamos procesando tu solicitud, en breve te notificaremos via correo electronico el resultado." });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, CriticalError=true, Message = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAPByReception(string Clave, string IdAnaliticoPago)
        {
            try
            {
                // verificamos que no exista una factura del analitico
                var _analiticopagoInvoiceExists = await _utility.GetItem<DataResult<AnaliticoPagoDto>>($"Invoice/GetInvoiceAPByReception/{IdAnaliticoPago}");
                if (_analiticopagoInvoiceExists.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = _analiticopagoInvoiceExists.Message });
                             
                var analitico = await _utility.GetItem<DataResult<AnaliticoPagoDto>>($"AnaliticoPago/GetAPByIdAnaliticoClaveAsync/{IdAnaliticoPago}/{Clave}");
                if (analitico.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = analitico.Message });

                return PartialView("_DatosFactura", analitico.Data);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener el copade, por favor intente más tarde." });
                throw;
            }
        }
    }
}
