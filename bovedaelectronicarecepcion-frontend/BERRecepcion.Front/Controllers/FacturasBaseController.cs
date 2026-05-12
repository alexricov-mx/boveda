using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    public class FacturasBaseController : Controller
    {
        private readonly IGenerals _generals;
        protected readonly IRestUtility _utility;
        public FacturasBaseController(IRestUtility utility, IGenerals generals)
        {
            _utility = utility;
            _generals = generals;
        }
        [HttpPost]
        public async Task<IActionResult> ValidarNotasCredito(IEnumerable<IFormFile> xmlNotasCredito, IFormFile xmlFactura, IFormFile xmlNotaCreditoActual, string nota)
        {
            try
            {
                if (xmlNotasCredito == null || xmlNotasCredito.Count() == 0 || string.IsNullOrWhiteSpace(nota) || xmlNotaCreditoActual == null)
                    return await Task.Run(() => Json(new { success = false, message = "El archivo ingresado no es correcto." }));
                var _notasCredito = await _generals.GetComprobantes(xmlNotasCredito, esNotaCredito: true);
                if (_notasCredito.Status != System.Net.HttpStatusCode.OK)
                    return await Task.Run(() => Json(new { success = false, message = _notasCredito.Message }));
                var _factura = await _generals.GetComprobante(xmlFactura, esFactura: true);
                var UUIDNotasCredito = _notasCredito.Data.Select(x => x.Complemento.TimbreFiscalDigital.UUID).ToList();
                if (_notasCredito.Data.Any(x => Guid.Parse(x.CfdiRelacionados.CfdiRelacionado.UUID) != _factura.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID))
                    return await Task.Run(() => Json(new { success = false, message = "La nota de crédito seleccionada no pertenece a la factura." }));
                if (_notasCredito.Data.Any(x => x.Moneda != _factura.Data.ComprobanteBE.Moneda))
                    return await Task.Run(() => Json(new { success = false, message = "La moneda de la nota de crédito seleccionada no corresponde con la de la factura." }));
                var _duplicadas = _notasCredito.Data.Select(x => x.Complemento.TimbreFiscalDigital.UUID).GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                if (xmlNotasCredito.Count() > 1 && _duplicadas.Count > 0)
                    return Json(new { success = false, message = "No es posible agregar más de una vez la misma nota de crédito." });
                var notaCredito = JsonConvert.DeserializeObject<CopadeNotaCredito>(nota);
                var _notaCreditoActual = await _generals.GetComprobante(xmlNotaCreditoActual, esNotaCredito: true);
                if (_notaCreditoActual.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID == _factura.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID)
                    return await Task.Run(() => Json(new { success = false, message = "El UUID de la nota de crédito es el mismo que el de la factura, favor verificar." }));
                if (!string.IsNullOrWhiteSpace(_notaCreditoActual.Data.ComprobanteBE.Fecha) && Convert.ToDateTime(_notaCreditoActual.Data.ComprobanteBE.Fecha) > DateTime.Now)
                    return await Task.Run(() => Json(new { success = false, message = "No es posible ingresar notas de crédito con fechas a futuro, favor verificar." }));
                if (string.IsNullOrWhiteSpace(_notaCreditoActual.Data.ComprobanteBE.Emisor.Rfc) || !_notaCreditoActual.Data.ComprobanteBE.Emisor.Rfc.Equals(_factura.Data.ComprobanteBE.Emisor.Rfc))
                    return await Task.Run(() => Json(new { success = false, message = "El RFC del emisor de la nota de crédito, no coincide con el de la factura." }));
                if (string.IsNullOrWhiteSpace(_notaCreditoActual.Data.ComprobanteBE.Receptor.Rfc) || !_notaCreditoActual.Data.ComprobanteBE.Receptor.Rfc.Equals(_factura.Data.ComprobanteBE.Receptor.Rfc))
                    return await Task.Run(() => Json(new { success = false, message = "El RFC del emisor de la nota de crédito, no coincide con el de la factura." }));
                if ((Math.Abs(Convert.ToDouble(_notaCreditoActual.Data.ComprobanteBE.Total) - Convert.ToDouble(notaCredito.Total)) >= Convert.ToDouble(1)))
                    return await Task.Run(() => Json(new { success = false, message = "El total del archivo seleccionado no corresponde al de la nota de crédito." }));
                if (_notaCreditoActual.Data.ComprobanteBE.Conceptos == null || _notaCreditoActual.Data.ComprobanteBE.Conceptos.Concepto.Count == 0)
                    return await Task.Run(() => Json(new { success = false, message = "No se encontraron conceptos en la nota de crédito." }));
                var conceptoNC = _notaCreditoActual.Data.ComprobanteBE.Conceptos.Concepto.FirstOrDefault();
                if (conceptoNC.Cantidad != int.Parse(notaCredito.Cantidad)
                    || _notaCreditoActual.Data.ComprobanteBE.SubTotal != double.Parse(notaCredito.Importe) //Importe
                    || _notaCreditoActual.Data.ComprobanteBE.Impuestos.TotalImpuestosTrasladados != double.Parse(notaCredito.Iva)) //IVA
                    return await Task.Run(() => Json(new { success = false, message = "El archivo seleccionado no corresponde con la nota de crédito." }));

                return await Task.Run(() => Json(new { success = true, message = "Nota de crédito agregada con éxito." }));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "El arhivo ingresado no es correcto." });
            }
        }

        
        public async Task<DataResult<CopadeDto>> ValidarFactura(ComprobanteBE comprobante, string clave = null)
        {
            DataResult<CopadeDto> result = new DataResult<CopadeDto>()
            {
                Status = System.Net.HttpStatusCode.BadRequest
            };
            try
            {
             
                if (clave is null)
                {
                    clave = _generals.User.Organisms.FirstOrDefault().Clave;
                }
                
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("Clave", clave));
                param.Add(new CustomHttpParameter("Reception", comprobante.Addenda.Addenda_Pemex.ENTRADA));
                param.Add(new CustomHttpParameter("Exercise", comprobante.Addenda.Addenda_Pemex.EJERCICIO));

                var _copadeInvoiceExists = await _utility.GetItem<DataResult<InvoiceDto>>("Invoice/GetInvoiceByReception", param);
                if (_copadeInvoiceExists.Status != System.Net.HttpStatusCode.OK)
                {
                    result.Message = _copadeInvoiceExists.Message;
                    return result;
                }
                
                var copade = await _utility.GetItem<DataResult<CopadeDto>>("Copade/GetCopadeByReceptionAsync", param);
                if (copade.Status != System.Net.HttpStatusCode.OK)
                {
                    result.Message = copade.Message;
                    return result;
                }
                if(!string.IsNullOrWhiteSpace(comprobante.Fecha) && Convert.ToDateTime(comprobante.Fecha) > DateTime.Now)
                {
                    result.Message = "No es posible agregar facturas con fechas a futuro.";
                    return result;
                }
                //if (comprobante.MetodoPago != "PPD")
                //{
                //    result.Message = "El método de pago de la factura es incorrecto.";
                //    return result;
                //}
                if (string.IsNullOrWhiteSpace(copade.Data.CreditorRfc) || !copade.Data.CreditorRfc.Equals(comprobante.Emisor.Rfc))
                {
                    result.Message = "No coincide el RFC del emisor.";
                    return result;
                }

                if (_generals.User.Organisms.Where(x => x.Rfc == comprobante.Receptor.Rfc).ToList().Count > 1)
                {
                    result.Message = "No coincide el RFC del receptor.";
                    return result;
                }
                if (copade.Data.IsCancel == true)
                {
                    result.Message = "El copade se encuentra cancelado.";
                    return result;
                }
                if (!Convert.ToBoolean(copade.Data.IsFullSigned))
                {
                    result.Message = "El copade no se encuentra complentamente firmado.";
                    return result;
                }

                if (comprobante.Conceptos.Concepto.Count() != copade.Data.vPreFactura.comprobante.conceptos.Count())
                {
                    result.Message = "El número de conceptos del copade no coincide con el de la factura.";
                    return result;
                }

                if (Math.Abs(Convert.ToDouble(comprobante.Total) - Convert.ToDouble(copade.Data.Total)) >= Convert.ToDouble(1))
                {
                    result.Message = "El total de la factura no coincide con el del copade.";
                    return result;
                }
                foreach (var item in comprobante.Conceptos.Concepto)
                {
                    var existeEnCopade = copade.Data.vPreFactura.comprobante.conceptos.Where(x =>
                    Convert.ToInt32(Math.Floor(Convert.ToDouble(x.cantidad))) == item.Cantidad
                    && (Math.Abs(Convert.ToDouble(x.valorUnitario) - Convert.ToDouble(item.ValorUnitario)) < Convert.ToDouble(1))
                    && (Math.Abs(Convert.ToDouble(x.importe) - Convert.ToDouble(item.Importe)) < Convert.ToDouble(1))
                    ).FirstOrDefault();
                    if (existeEnCopade == null)
                    {
                        result.Message = "Los conceptos del copade no coinciden con los de la factura seleccionada.";
                        return result;
                    }
                }
                if (comprobante.Complemento.CartaPorte != null)
                {
                    var cartaPorte = await _utility.Post(new DataResult<CartaPorte> { Data = comprobante.Complemento.CartaPorte }, "SAT/Catalogos/GetCartaPorteDireccionesAsync");
                    comprobante.Complemento.CartaPorte = cartaPorte.Data;
                }
                copade.Data.Comprobante = comprobante;
                result.Status = System.Net.HttpStatusCode.OK;
                result.Data = copade.Data;
                return result;
            }
            catch (Exception)
            {
                result.Message = "Ocurrió un error al obtener los datos de la factura.";
                return result;
            }
        }
        [HttpPost]
        public async Task<IActionResult> ValidationErrors(IEnumerable<ValidationError> errors)
        {
            return await Task.Run(() => PartialView("_ValidationErrors", errors));
        }
    }
}
