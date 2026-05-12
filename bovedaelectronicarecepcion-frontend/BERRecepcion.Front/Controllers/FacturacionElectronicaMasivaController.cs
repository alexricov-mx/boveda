using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class FacturacionElectronicaMasivaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IGenerals _generals;
        private readonly IRestUtility _utility;
        public FacturacionElectronicaMasivaController(IConfiguration configuration, IGenerals generals, IRestUtility utility)
        {
            _generals = generals;
            _configuration = configuration;
            _utility = utility;
        }
        [RoleFilter("ReceptionElectronicMultipleInvoice")]
        public IActionResult Index()
        {
            return View();
        }
       
        [HttpPost]
        public async Task<IActionResult> ValidarArchivos(IEnumerable<IFormFile> facturas, IEnumerable<IFormFile> notasCredito, bool enviar = false)
        {
            if (facturas == null || facturas.Count() == 0)
                return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
            var _facturasXML = await _generals.GetComprobantes(facturas, esFactura: true);
            if (_facturasXML.Status != System.Net.HttpStatusCode.OK)
                return Json(new { success = false, message = _facturasXML.Message });
            var _notasCreditoXML = await _generals.GetComprobantes(notasCredito, esNotaCredito: true);

            if (facturas.Count() == 1 && !_facturasXML.Data.First().TipoDeComprobante.Equals("I"))
                return Json(new { success = false, message = "El archivo que ha seleccionado no corresponde a una factura, favor de verificar." });

            List<Guid> uuidFacturasDuplicadas = new List<Guid>();
            List<Guid> uuidNotasDuplicadas = new List<Guid>();

            List<ComprobanteDto> comprobantes = new List<ComprobanteDto>();
            List<ComprobanteDto> comprobantesSinError = new List<ComprobanteDto>();
            List<ComprobanteDto> comprobantesConError = new List<ComprobanteDto>();

            try
            {
                foreach (IFormFile factura in facturas)
                {
                    var notasCreditoBDList = new List<CopadeNotaCredito>();
                    ComprobanteDto comprobanteDto = new ComprobanteDto();
                    var _comprobante = await _generals.GetComprobante(factura, esFactura: true);
                    if (_comprobante.Status != System.Net.HttpStatusCode.OK)
                    {
                        comprobanteDto.existedException = true;
                        comprobanteDto.comprobante = _comprobante.Data.ComprobanteBE;
                        comprobanteDto.exceptionMessage = _comprobante.Message;
                    }
                    if (uuidFacturasDuplicadas.Where(x => x == _comprobante.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID).FirstOrDefault() != Guid.Empty)
                    {
                        comprobanteDto.existedException = true;
                        comprobanteDto.comprobante = _comprobante.Data.ComprobanteBE;
                        comprobanteDto.exceptionMessage = "El UUID de la factura se encuentra duplicado con el de alguna factura.";
                    }
                    uuidFacturasDuplicadas.Add(_comprobante.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID);
                    if (_comprobante.Data.ComprobanteBE.Addenda == null || _comprobante.Data.ComprobanteBE.Addenda.Addenda_Pemex == null)
                    {
                        comprobanteDto.existedException = true;
                        comprobanteDto.comprobante = _comprobante.Data.ComprobanteBE;
                        comprobanteDto.exceptionMessage = "La factura no contiene Addenda, o esta no tiene el formato correcto.";
                    }

                    if (uuidNotasDuplicadas.Where(x => x == _comprobante.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID).FirstOrDefault() != Guid.Empty)
                    {
                        comprobanteDto.existedException = true;
                        comprobanteDto.comprobante = _comprobante.Data.ComprobanteBE;
                        comprobanteDto.exceptionMessage = "El UUID de la factura se encuentra duplicado con el de alguna nota de crédito.";
                    }
                    var copade = await new FacturasBaseController(_utility, _generals).ValidarFactura(_comprobante.Data.ComprobanteBE);
                    if (copade.Status != System.Net.HttpStatusCode.OK)
                    {
                        comprobanteDto.existedException = true;
                        comprobanteDto.comprobante = _comprobante.Data.ComprobanteBE;
                        comprobanteDto.exceptionMessage = copade.Message;
                    }
                    var _notasCredito = new List<ComprobanteBE>();
                    var _notasCreditoTemp = new List<ComprobanteBE>();
                    var _notasCreditoCFDI = new List<NotaCreditoCFDIXMLDto>();
                    var _notasCreditoFactura = _notasCreditoXML.Data.Where(x => Guid.Parse(x.CfdiRelacionados.CfdiRelacionado.UUID) == _comprobante.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID).ToList();
                    var notasCreditoFacturaTemp = _notasCreditoFactura;
                    foreach (var item in notasCredito)
                    {
                        var _notaCredito = await _generals.GetComprobante(item, esNotaCredito: true);

                        if (_notasCreditoFactura.Where(x => x.Complemento.TimbreFiscalDigital.UUID == _notaCredito.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID).FirstOrDefault() != null)
                        {
                            _notasCredito.Add(_notaCredito.Data.ComprobanteBE);
                            var _notaCreditoCFDI = new NotaCreditoCFDIXMLDto
                            {
                                OriginalXML = await _generals.ReadFileAsync(item),
                                UUID = _notaCredito.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID
                            };
                            _notasCreditoCFDI.Add(_notaCreditoCFDI);
                        }
                    }
                    if (copade != null && copade.Data != null && copade.Data.vNotasCredito != null)
                        foreach (var item in copade.Data.vNotasCredito?.notaCredito.ToList())
                        {
                            _notasCreditoTemp = _notasCredito;
                            var notaCreditoFactura = _notasCreditoFactura.Where(x =>
                                x.Conceptos.Concepto.FirstOrDefault().Cantidad == int.Parse(item.Cantidad)
                                && (Math.Abs(Convert.ToDouble(x.SubTotal) - Convert.ToDouble(item.Importe)) < Convert.ToDouble(1))
                                && (Math.Abs(Convert.ToDouble(x.Impuestos.TotalImpuestosTrasladados) - Convert.ToDouble(item.Iva)) < Convert.ToDouble(1))
                                && (Math.Abs(Convert.ToDouble(x.Total) - Convert.ToDouble(item.Total)) < Convert.ToDouble(1))).FirstOrDefault();
                            if (notaCreditoFactura != null)
                            {
                                item.Id = notaCreditoFactura.Complemento.TimbreFiscalDigital.UUID;
                                var validarNota = ValidarNotaCredito(_notasCreditoXML.Data, notasCreditoFacturaTemp, notaCreditoFactura, item, _comprobante.Data.ComprobanteBE);

                                item.HasError = validarNota.Status != System.Net.HttpStatusCode.OK;
                                item.ErrorMessage = validarNota.Message;
                                if (uuidFacturasDuplicadas.Any(x => x == notaCreditoFactura.Complemento.TimbreFiscalDigital.UUID))
                                {
                                    item.HasError = true;
                                    item.ErrorMessage = "El UUID de nota de crédito se encuentra duplicado con el de alguna factura.";
                                }
                                uuidNotasDuplicadas.Add(notaCreditoFactura.Complemento.TimbreFiscalDigital.UUID);
                                if (item.HasError && enviar)
                                {
                                    var notaToRemove = _notasCredito.Where(x => x.Complemento.TimbreFiscalDigital.UUID == notaCreditoFactura.Complemento.TimbreFiscalDigital.UUID).FirstOrDefault();
                                    _notasCreditoTemp.Remove(notaToRemove);
                                }
                                _notasCreditoFactura.Remove(notaCreditoFactura);
                            }
                            notasCreditoBDList.Add(item);
                        }
                    if (notasCreditoBDList.Where(x => x.HasError).Count() > 0)
                    {
                        comprobanteDto.existedException = true;
                        comprobanteDto.comprobante = _comprobante.Data.ComprobanteBE;
                        comprobanteDto.notasCredito = _notasCredito;
                        comprobanteDto.notasCreditoBD = notasCreditoBDList;
                        comprobanteDto.exceptionMessage = "Existen errores de validación en las notas de crédito.";
                    }
                    if (comprobanteDto.existedException)
                        comprobantesConError.Add(comprobanteDto);
                    else
                    {
                        comprobanteDto.comprobante = _comprobante.Data.ComprobanteBE;
                        comprobanteDto.User = _generals.User;
                        comprobanteDto.claveOrganismo = _generals.User.Organisms.FirstOrDefault().Clave;
                        comprobanteDto.notasCredito = enviar ? _notasCreditoTemp : _notasCredito;
                        comprobanteDto.notasCreditoBD = notasCreditoBDList;
                        comprobanteDto.esCopade = true;
                        comprobanteDto.OriginalXml = await _generals.ReadFileAsync(factura);
                        comprobanteDto.notasCreditoCFDIXML = _notasCreditoCFDI;
                        comprobantesSinError.Add(comprobanteDto);
                    }

                    if (enviar && _notasCreditoTemp.Count != notasCreditoBDList.Count)
                        return Json(new { success = false, message = $"No se han agregado las notas de crédito correspondientes para la factura {_comprobante.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID}" });
                }

                if (!enviar)
                {
                    ViewBag.Emisor = _facturasXML.Data.FirstOrDefault().Emisor;
                    ViewBag.Receptor = _generals.User.Organisms.FirstOrDefault();
                    ViewBag.ContieneError = comprobantesConError.Count > 0;
                    comprobantes.AddRange(comprobantesSinError);
                    comprobantes.AddRange(comprobantesConError);
                    return PartialView("_InfoFacturas", comprobantes);
                }

                await _utility.Post(comprobantesSinError, "CFDI/SaveInvoiceMultiple");
                return Json(new { success = true, result = comprobantesSinError });
            }
            catch (Exception )
            {
                return Json(new { success = false, message = "Ocurrió un error inesperado, favor de intentar más tarde." });
            }
        }
        private DataResult<ComprobanteBE> ValidarNotaCredito(IEnumerable<ComprobanteBE> notasCreditoFacturas, IEnumerable<ComprobanteBE> notasCreditoFactura, ComprobanteBE notaCreditoActual, CopadeNotaCredito notaCreditoCopade, ComprobanteBE factura)
        {
            var result = new DataResult<ComprobanteBE> { Status = System.Net.HttpStatusCode.BadRequest, Data = notaCreditoActual };
            try
            {
                var UUIDNotasCredito = notasCreditoFactura.Select(x => x.Complemento.TimbreFiscalDigital.UUID).ToList();
                if (notasCreditoFactura.Any(x => Guid.Parse(x.CfdiRelacionados.CfdiRelacionado.UUID) != factura.Complemento.TimbreFiscalDigital.UUID))
                {
                    result.Message = "La nota de crédito seleccionada no pertenece a la factura.";
                    return result;
                }
                if (notasCreditoFactura.Any(x => x.Moneda != factura.Moneda))
                {
                    result.Message = "La moneda de la nota de crédito seleccionada no corresponde con la de la factura.";
                    return result;
                }
                var _duplicadas = notasCreditoFactura.Select(x => x.Complemento.TimbreFiscalDigital.UUID).GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                if (notasCreditoFactura.Count() > 1 && _duplicadas.Count > 0)
                {
                    result.Message = "No es posible agregar más de una vez la misma nota de crédito.";
                    return result;
                }
                var _duplicadasAll = notasCreditoFacturas.Select(x => x.Complemento.TimbreFiscalDigital.UUID).GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                if (notasCreditoFacturas.Count() > 1 && _duplicadasAll.Count > 0)
                {
                    result.Message = "La nota de crédito se encuentra duplicada con el de otra nota de crédito.";
                    return result;
                }
                if (notaCreditoActual.Complemento.TimbreFiscalDigital.UUID == factura.Complemento.TimbreFiscalDigital.UUID)
                {
                    result.Message = "El UUID de la nota de crédito es el mismo que el de la factura.";
                    return result;
                }
                if (!string.IsNullOrWhiteSpace(notaCreditoActual.Fecha) && Convert.ToDateTime(notaCreditoActual.Fecha) > DateTime.Now)
                {
                    result.Message = "No es posible ingresar notas de crédito con fechas a futuro.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(notaCreditoActual.Emisor.Rfc) || !notaCreditoActual.Emisor.Rfc.Equals(factura.Emisor.Rfc))
                {
                    result.Message = "El RFC del emisor de la nota de crédito no coincide con el de la factura.";
                    return result;
                }
                if (string.IsNullOrWhiteSpace(notaCreditoActual.Receptor.Rfc) || !notaCreditoActual.Receptor.Rfc.Equals(factura.Receptor.Rfc))
                {
                    result.Message = "El RFC del receptor de la nota de crédito no coincide con el de la factura.";
                    return result;
                }
                if ((Math.Abs(Convert.ToDouble(notaCreditoActual.Total) - Convert.ToDouble(notaCreditoCopade.Total)) >= Convert.ToDouble(1)))
                {
                    result.Message = "El total de archivo seleccionado no corresponde al de la nota de crédito.";
                    return result;
                }
                if (notaCreditoActual.Conceptos == null || notaCreditoActual.Conceptos.Concepto.Count == 0)
                {
                    result.Message = "No se encontraron conceptos en la nota de crédito.";
                    return result;
                }
                var conceptoNC = notaCreditoActual.Conceptos.Concepto.FirstOrDefault();
                if (conceptoNC.Cantidad != int.Parse(notaCreditoCopade.Cantidad)
                    || notaCreditoActual.SubTotal != double.Parse(notaCreditoCopade.Importe) //Importe
                    || notaCreditoActual.Impuestos.TotalImpuestosTrasladados != double.Parse(notaCreditoCopade.Iva)) //IVA
                {
                    result.Message = "El archivo seleccionado no corresponde con la nota de crédito.";
                    return result;
                }
                result.Status = System.Net.HttpStatusCode.OK;
                return result;
            }
            catch (Exception )
            {
                result.Message = "Ocurrió un error inesperado, favor de contactar a su administrador.";
                return result;
            }
        }
    }
}
