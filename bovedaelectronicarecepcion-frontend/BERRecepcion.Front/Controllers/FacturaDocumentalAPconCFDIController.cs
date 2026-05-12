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
    public class FacturaDocumentalAPconCFDIController : Controller
    {
        private readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public FacturaDocumentalAPconCFDIController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        public async Task<IActionResult> Index()
        {
            return await Task.Run(() => View(_generals.User.Organisms.GroupBy(x => x.Name).Select(group => group.First())));
        }
        [HttpGet]
        public async Task<IActionResult> GetAPByReception(string Clave, string IdAnaliticoPago)
        {
            try
            {
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
                return Json(new { success = false, message = "Ocurrió un error al obtener el analitico de pago, por favor intente más tarde." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> InfoFactura(IFormFile factura)
        {
            try
            {
                if (factura == null)
                    return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
                var _comprobante = await _generals.GetComprobante(factura, esFactura: true);
                if (_comprobante.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = _comprobante.Message });
                return await Task.Run(() => PartialView("_InfoFactura", _comprobante.Data.ComprobanteBE));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener los datos de la factura, por favor intente más tarde." });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> EnviarFactura(IFormFile factura, string ViaPago, string IdAnaliticoPago, string Clave, string RFCReceptor, string DocumentoBEId)
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
                string resultMessage = "";          

                InvoiceResultDto _dto = new InvoiceResultDto();
                _dto.Organismo = Clave;
                _dto.IdDocumento = IdAnaliticoPago;
                _dto.RFCReceptor = RFCReceptor;
                _dto.User = _generals.User;
                _dto.comprobante = null;
                _dto.ViaPago = ViaPago;
                _dto.DocumentoBEId = DocumentoBEId;
                _dto.OriginalXML = await _generals.ReadFileAsync(factura);
                _dto.ComprobanteBEString = JsonConvert.SerializeObject(comprobante);
                _dto.ComprobanteOriginal = _comprobante.Data.ComprobanteOriginal;

                var result = await _utility.Post(_dto, "FacturaDocumental/SaveInvoiceComprobante");

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
                return Json(new { Success = false, CriticalError= true, Message = "Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador." });
            }
        }
        private async Task<DataResult<AnaliticoPagoDto>> ValidarFactura(ComprobanteBE comprobante, string IdAnaliticoPago, string Clave)
        {
            DataResult<AnaliticoPagoDto> result = new DataResult<AnaliticoPagoDto>()
            {
                Status = System.Net.HttpStatusCode.BadRequest
            };
            try
            {
                var _analiticopagoInvoiceExists = await _utility.GetItem<DataResult<AnaliticoPagoDto>>($"Invoice/GetInvoiceAPByReception/{IdAnaliticoPago}");
                if (_analiticopagoInvoiceExists.Status != System.Net.HttpStatusCode.OK)
                {
                    result.Message = _analiticopagoInvoiceExists.Message;
                    return result;
                }
                var analitico = await _utility.GetItem<DataResult<AnaliticoPagoDto>>($"AnaliticoPago/GetAPByIdAnaliticoClaveAsync/{IdAnaliticoPago}/{Clave}");
                if (analitico.Status != System.Net.HttpStatusCode.OK)
                {
                    result.Message = analitico.Message;
                    return result;
                }
                
                if (!_generals.User.Organisms.FirstOrDefault().Rfc.Equals(comprobante.Receptor.Rfc))
                {
                    result.Message = "No coincide el RFC del receptor, favor de verificar.";
                    return result;
                }
                if (analitico.Data.IsCancel == true)
                {
                    result.Message = "El analitico se encuentra cancelado, favor de verificar.";
                    return result;
                }
                
                if (comprobante.Conceptos.Concepto.Count() != analitico.Data.vPreFactura.comprobante.conceptos.Count())
                {
                    result.Message = "El número de conceptos del analitico no coincide con el de la factura.";
                    return result;
                }

                if (Math.Abs(Convert.ToDouble(comprobante.Total) - Convert.ToDouble(analitico.Data.Total)) >= Convert.ToDouble(1))
                {
                    result.Message = "El total de la factura no coincide con el del copade, favor de verificar.";
                    return result;
                }
                foreach (var item in comprobante.Conceptos.Concepto)
                {
                    var existeEnCopade = analitico.Data.vPreFactura.comprobante.conceptos.Where(x =>
                    Convert.ToInt32(Math.Floor(Convert.ToDouble(x.cantidad))) == item.Cantidad
                    && (Math.Abs(Convert.ToDouble(x.valorUnitario) - Convert.ToDouble(item.ValorUnitario)) < Convert.ToDouble(1))
                    && (Math.Abs(Convert.ToDouble(x.importe) - Convert.ToDouble(item.Importe)) < Convert.ToDouble(1))
                    ).FirstOrDefault();
                    if (existeEnCopade == null)
                    {
                        result.Message = "Los conceptos del analitico no coinciden con los de la factura seleccionada, favor de verificar.";
                        return result;
                    }
                }
                //analitico.Data.Comprobante = comprobante;
                result.Status = System.Net.HttpStatusCode.OK;
                result.Data = analitico.Data;
                return result;
            }
            catch (Exception)
            {
                result.Message = "Ocurrió un error al obtener los datos de la factura.";
                return result;
            }
        }
    }
}
