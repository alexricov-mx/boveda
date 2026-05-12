using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using DocumentFormat.OpenXml.Spreadsheet;
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
    [ValidateUser]
    public class FacturaDocumentalsinCFDIController : Controller
    {
        private readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public FacturaDocumentalsinCFDIController(IConfiguration configuration, IRestUtility utility, IGenerals generals, IRestUtility rest)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        [RoleFilter("ReceptionDocumentalInvoice")]
        public async Task<IActionResult> Index()
        {
            return await Task.Run(() => View(_generals.User.Organisms.GroupBy(x => x.Name).Select(group => group.First())));
        }
        /// <summary>
        /// Recupera el copade
        /// </summary>
        /// <param name="Clave"></param>
        /// <param name="Reception"></param>
        /// <param name="Exercise"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCopadeByReception(string Clave, string Reception, string Exercise)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("Clave", Clave));
                param.Add(new CustomHttpParameter("Reception", Reception));
                param.Add(new CustomHttpParameter("Exercise", Exercise));
                var _copadeInvoiceExists = await _utility.GetItem<DataResult<CopadeDto>>("Invoice/GetInvoiceByReception", param);
                if (_copadeInvoiceExists.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = _copadeInvoiceExists.Message });
                
                var copade = await _utility.GetItem<DataResult<CopadeDto>>("Copade/GetCopadeByReceptionAsync", param);
                if (copade.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = copade.Message });
                return PartialView("_DatosFactura", copade.Data);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener el copade, por favor intente más tarde." });
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveInvoice(string Organismo, string Reception, string Exercise, string RFCReceptor, string Correo, InvoiceDto dto)
        {
            try
            {
                string resultMessage = "";
                bool success = false;
                bool criticalError = false;

                ComprobanteBE comprobante = new ComprobanteBE();
                comprobante.Addenda = new Addenda();
                comprobante.Addenda.Addenda_Pemex = new Addenda_Pemex();
                comprobante.Addenda.Addenda_Pemex.EJERCICIO = Exercise;

                InvoiceResultDto _dto = new InvoiceResultDto();
                _dto.Organismo = Organismo;
                _dto.IdDocumento = Reception;
                _dto.RFCReceptor = RFCReceptor;
                _dto.InvoiceDto = dto;
                _dto.User = _generals.User;
                _dto.Correo = Correo;
                _dto.comprobante = comprobante;
                
                var result = await _utility.Post(_dto, "FacturaDocumental/SaveInvoiceSinCFDI");

                if (result != null)
                {
                    if (result.ExistedException)
                    {
                        resultMessage = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                        success = false;
                        criticalError = true;
                    }
                    else
                    {
                        List<ValidationError> validationErrors = result.validationErrors.ToList();
                        if (validationErrors.Count == 0)
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
                return Json(new { Success = false, CriticalError = true, isTimeOut = true, Message = "Continuamos procesando tu solicitud, en breve te notificaremos via correo electronico el resultado." });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, CriticalError = true, Message = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }
    }
}
