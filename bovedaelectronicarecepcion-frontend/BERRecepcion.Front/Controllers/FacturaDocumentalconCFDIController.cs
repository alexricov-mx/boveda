using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
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
    [ValidateUser]
    public class FacturaDocumentalconCFDIController : Controller
    {
        private readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        public FacturaDocumentalconCFDIController(IConfiguration configuration, IRestUtility utility, IGenerals generals, IRestUtility rest)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        [RoleFilter("ReceptionDocumentalInvoiceFromXML")]
        public async Task<IActionResult> Index()
        {
            return await Task.Run(() => View(_generals.User.Organisms.GroupBy(x => x.Name).Select(group => group.First())));
        }
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
            }
        }
        [HttpPost]
        public async Task<IActionResult> InfoFactura(IFormFile factura, string entrada, string ejercicio)
        {
            try
            {
                if (factura == null)
                    return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
                var _comprobante = await _generals.GetComprobante(factura, esFactura: true);
                if (_comprobante.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = _comprobante.Message });
                _comprobante.Data.ComprobanteBE.Addenda = new Addenda() { Addenda_Pemex = new Addenda_Pemex() { ENTRADA = entrada, EJERCICIO = ejercicio } };
                var copade = await new FacturasBaseController(_utility, _generals).ValidarFactura(_comprobante.Data.ComprobanteBE);
                if (copade.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = copade.Message });
                return PartialView("_InfoFactura", _comprobante.Data.ComprobanteBE);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener los datos de la factura, por favor intente más tarde." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnviarFactura(IFormFile factura, IEnumerable<IFormFile> notasCredito, string ViaPago, string Correo, string CopadeId, string Clave, string RFCReceptor, string DocumentoBEId)
        {
            if (factura == null)
                return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
            var _comprobante = await _generals.GetComprobante(factura, esFactura: true);
            if (_comprobante.Status != System.Net.HttpStatusCode.OK)
                return Json(new { success = false, message = _comprobante.Message });
            ComprobanteBE comprobante = _comprobante.Data.ComprobanteBE;
            bool success = false;
            bool criticalError = false;

            try
            {
                var _notasCredito = new List<ComprobanteBE>();
                var _notasCreditoCFDI = new List<NotaCreditoCFDIXMLDto>();
                foreach (var item in notasCredito)
                {
                    var _notaCredito = await _generals.GetComprobante(item, esNotaCredito: true);
                    if (_notaCredito.Status != System.Net.HttpStatusCode.OK)
                        return Json(new { success = false, message = _notaCredito.Message });
                    _notasCredito.Add(_notaCredito.Data.ComprobanteBE);
                    var _notaCreditoCFDI = new NotaCreditoCFDIXMLDto
                    {
                        OriginalXML = await _generals.ReadFileAsync(item),
                        UUID = _notaCredito.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID
                    };
                    _notasCreditoCFDI.Add(_notaCreditoCFDI);
                }

                string resultMessage = "";

                InvoiceResultDto _dto = new InvoiceResultDto();
                _dto.Organismo = Clave;
                _dto.IdDocumento = CopadeId;
                _dto.RFCReceptor = RFCReceptor;
                _dto.User = _generals.User;
                _dto.comprobante = null;
                _dto.ViaPago = ViaPago;
                _dto.DocumentoBEId = DocumentoBEId;
                _dto.OriginalXML = await _generals.ReadFileAsync(factura);
                _dto.notasCredito = _notasCredito;
                _dto.notasCreditoCFDI = _notasCreditoCFDI;
                _dto.Correo = Correo;
                _dto.ComprobanteBEString = JsonConvert.SerializeObject(comprobante);
                _dto.ComprobanteOriginal = _comprobante.Data.ComprobanteOriginal;

                var result = await _utility.Post(_dto, "FacturaDocumental/SaveInvoiceConCFDI");

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
