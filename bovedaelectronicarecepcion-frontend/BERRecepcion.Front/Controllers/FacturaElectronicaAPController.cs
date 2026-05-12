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
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BERRecepcion.Front.Controllers
{
    [AllowAnonymous]
    public class FacturaElectronicaAPController : Controller
    {
        private readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public FacturaElectronicaAPController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        public IActionResult Index()
        {
            return View(_generals.User.Organisms.GroupBy(x => x.Name).Select(group => group.First()));
        }
        public async Task<IActionResult> ExisteAddenda(IFormFile factura)
        {
            if (factura == null)
                return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
            var _comprobante = await _generals.GetComprobante(factura, esFactura: true);
            if (_comprobante.Status != System.Net.HttpStatusCode.OK || _comprobante.Data == null)
                return Json(new { success = false, message = _comprobante.Message });
            return PartialView("_TextAddenda", (_comprobante.Data.ComprobanteBE.Addenda != null && _comprobante.Data.ComprobanteBE.Addenda.Addenda_Pemex != null));
        }

        [HttpPost]
        public async Task<IActionResult> Validar(IFormFile factura, string addenda = "")
        {
            if (factura == null)
                return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
            var _comprobante = await _generals.GetComprobante(factura, esFactura: true);
            if (_comprobante.Status != System.Net.HttpStatusCode.OK)
                return Json(new { success = false, message = _comprobante.Message, criticalError = true });
            ComprobanteBE comprobante = _comprobante.Data.ComprobanteBE;
            if (!string.IsNullOrWhiteSpace(_comprobante.Data.ComprobanteBE.Fecha) && Convert.ToDateTime(_comprobante.Data.ComprobanteBE.Fecha) > DateTime.Now)
                return Json(new { success = false, message = "No es posible ingresar facturas con fechas a futuro, favor de verificar.", criticalError = true });
            //Validar si el comprobante contiene addenda
            if (comprobante.Addenda == null || comprobante.Addenda.Addenda_Pemex == null)
            {
                var _addenda = _generals.DeserializeAddenda(System.Web.HttpUtility.HtmlDecode(addenda));
                if (_addenda.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = _addenda.Message, criticalError = true });
                comprobante.Addenda = _addenda.Data;
            }
            try
            {
                var analitico = await ValidarFacturaAP(comprobante);
                if (analitico.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = analitico.Message, criticalError = true });
                return PartialView("_DatosFactura", analitico.Data);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener los datos de la factura.", criticalError = true });
            }
        }

        public async Task<DataResult<AnaliticoPagoDto>> ValidarFacturaAP(ComprobanteBE comprobante)
        {
            DataResult<AnaliticoPagoDto> result = new DataResult<AnaliticoPagoDto>()
            {
                Status = System.Net.HttpStatusCode.BadRequest
            };
            try
            {
                var _analiticopagoInvoiceExists = await _utility.GetItem<DataResult<AnaliticoPagoDto>>($"Invoice/GetInvoiceAPByReception/{comprobante.Addenda.Addenda_Pemex.ID_ANALITICO}");
                if (_analiticopagoInvoiceExists.Status != System.Net.HttpStatusCode.OK)
                {
                    result.Message = _analiticopagoInvoiceExists.Message;
                    return result;
                }

                var analitico = await _utility.GetItem<DataResult<AnaliticoPagoDto>>($"AnaliticoPago/GetAPByIdAnaliticoClaveAsync/{comprobante.Addenda.Addenda_Pemex.ID_ANALITICO}/{_generals.User.Organisms.FirstOrDefault().Clave}");
                if (analitico.Status != System.Net.HttpStatusCode.OK)
                {
                    result.Message = analitico.Message;
                    return result;
                }

                //if (comprobante.MetodoPago != "PPD")
                //{
                //    result.Message = "El método de pago de la factura es incorrecto, favor de verificar.";
                //    return result;
                //}
                if (string.IsNullOrWhiteSpace(analitico.Data.CreditorRFC) || !analitico.Data.CreditorRFC.Equals(comprobante.Emisor.Rfc))
                {
                    result.Message = "No coincide el RFC del emisor, favor de verificar.";
                    return result;
                }

                if (!_generals.User.Organisms.FirstOrDefault().Rfc.Equals(comprobante.Receptor.Rfc))
                {
                    result.Message = "No coincide el RFC del receptor, favor de verificar.";
                    return result;
                }
                if (analitico.Data.IsCancel == true)
                {
                    result.Message = "El Analitico de Pago se encuentra cancelado, favor de verificar.";
                    return result;
                }
                if (!Convert.ToBoolean(analitico.Data.FunctionarySignDate!=null))
                {
                    result.Message = "El Analitico de Pago no se encuentra complentamente firmado, favor de verificar.";
                    return result;
                }

                if (comprobante.Conceptos.Concepto.Count() != analitico.Data.vPreFactura.comprobante.conceptos.Count())
                {
                    result.Message = "El número de conceptos del Analitico de Pago no coincide con el de la factura.";
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
                        result.Message = "Los conceptos del Analitico de Pago no coinciden con los de la factura seleccionada, favor de verificar.";
                        return result;
                    }
                }
                analitico.Data.Comprobante = comprobante;
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


        [HttpPost]
        public async Task<IActionResult> EnviarFactura(IFormFile factura, IEnumerable<IFormFile> notasCredito, string documentoBEId, string viaPago, string addenda = "")
        {
            if (factura == null)
                return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
            var _comprobante = await _generals.GetComprobante(factura, esFactura: true);
            if (_comprobante.Status != System.Net.HttpStatusCode.OK)
                return Json(new { success = false, message = _comprobante.Message });
            ComprobanteBE comprobante = _comprobante.Data.ComprobanteBE;
            //Validar si el comprobante contiene addenda
            if (comprobante.Addenda == null || comprobante.Addenda.Addenda_Pemex == null)
            {
                var _addenda = _generals.DeserializeAddenda(System.Web.HttpUtility.HtmlDecode(addenda));
                if (_addenda.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = _addenda.Message });
                comprobante.Addenda = _addenda.Data;
            }
            try
            {
                var analitico = await ValidarFacturaAP(comprobante);
                if (analitico.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = analitico.Message });
                var _notasCredito = new List<ComprobanteBE>();
                var _notasCreditoCFDI = new List<NotaCreditoCFDIXMLDto>();

                string resultMessage = "";
                bool success = true;
                bool criticalError = false;

                InvoiceResultDto _dto = new InvoiceResultDto();
                _dto.Organismo = _generals.User.Organisms.FirstOrDefault().Clave;
                _dto.IdDocumento = comprobante.Addenda.Addenda_Pemex.ID_ANALITICO;
                _dto.RFCReceptor = comprobante.Receptor.Rfc;
                _dto.User = _generals.User;
                _dto.comprobante = null;
                _dto.DocumentoBEId = documentoBEId;
                _dto.ViaPago = viaPago;
                _dto.OriginalXML = await _generals.ReadFileAsync(factura);
                _dto.ComprobanteBEString = JsonConvert.SerializeObject(comprobante);
                _dto.ComprobanteOriginal = _comprobante.Data.ComprobanteOriginal;
                _dto.CFDIVersion = _comprobante.Data.ComprobanteBE.Version;

                var result = await _utility.Post(_dto, "CFDI/SaveInvoiceAP");

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
                return Json(new { Success = false, isTimeOut = true, CriticalError = true, Message = "Continuamos procesando tu solicitud, en breve te notificaremos via correo electronico el resultado." });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, CriticalError = true, Message = "Ocurrió un error al enviar la factura, por favor intente más tarde." });
            }
        }
    }
}
