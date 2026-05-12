using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class FacturaElectronicaController : Controller
    {
        private readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public FacturaElectronicaController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        [RoleFilter("ReceptionElectronicInvoice")]
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
                return Json(new { success = false, message = _comprobante.Message });
            ComprobanteBE comprobante = _comprobante.Data.ComprobanteBE;
            if (!string.IsNullOrWhiteSpace(_comprobante.Data.ComprobanteBE.Fecha) && Convert.ToDateTime(_comprobante.Data.ComprobanteBE.Fecha) > DateTime.Now)
                return Json(new { success = false, message = "No es posible ingresar facturas con fechas a futuro, favor de verificar." });
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
                var copade = await new FacturasBaseController(_utility, _generals).ValidarFactura(comprobante, _generals.User.Organisms.Where(x => x.CreditorRFC == _comprobante.Data.ComprobanteBE.Emisor.Rfc).FirstOrDefault().Clave);
                if (copade.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = copade.Message });
                //ViewBag.MultipleLocations = GetCartaPorteMultipleLocations(comprobante.Complemento.CartaPorte);
                return PartialView("_DatosFactura", copade.Data);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al obtener los datos de la factura." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> EnviarFactura(IFormFile factura, IEnumerable<IFormFile> notasCredito, string addenda = "")
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
                var copade = await new FacturasBaseController(_utility, _generals).ValidarFactura(comprobante, _generals.User.Organisms.Where(x => x.CreditorRFC == _comprobante.Data.ComprobanteBE.Emisor.Rfc).FirstOrDefault().Clave);
                if (copade.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = copade.Message });
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

                ComprobanteDto comprobanteDto = new ComprobanteDto()
                {
                    ComprobanteBEString = JsonConvert.SerializeObject(comprobante),
                    ComprobanteOriginal = _comprobante.Data.ComprobanteOriginal,
                    User = _generals.User,
                    claveOrganismo = _generals.User.Organisms.Where(x => x.CreditorRFC == _comprobante.Data.ComprobanteBE.Emisor.Rfc).FirstOrDefault().Clave,
                    notasCredito = _notasCredito,
                    esCopade = true,
                    OriginalXml = await _generals.ReadFileAsync(factura),
                    notasCreditoCFDIXML = _notasCreditoCFDI
                };
                return Json(new { success = true, result = comprobanteDto });
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, CriticalError = true, isTimeOut = true, Message = "Continuamos procesando tu solicitud, en breve te notificaremos via correo electronico el resultado." });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, CriticalError = true, Message = "Ocurrió un error al enviar la factura, por favor intente más tarde." });
            }
        }
        private List<CartaPorteViewModel> GetCartaPorteMultipleLocations(CartaPorte model)
        {
            List<CartaPorteViewModel> result = new List<CartaPorteViewModel>();
            foreach (var mercancia in model.Mercancias.Mercancia)
            {
                var origen = model.Ubicaciones.Where(x => x.IDUbicacion == mercancia.CantidadTransporta.FirstOrDefault()?.IDOrigen).FirstOrDefault();
                var destino = model.Ubicaciones.Where(x => x.IDUbicacion == mercancia.CantidadTransporta.FirstOrDefault()?.IDDestino).FirstOrDefault();
                var _addItem = new CartaPorteViewModel
                {
                    Mercancia = mercancia,
                    Autotransporte = model.Mercancias.Autotransporte,
                    FiguraTransporte = model.FiguraTransporte[0],
                    Origen = origen,
                    Destino = destino
                };
                result.Add(_addItem);
            }
            return result;
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceDto>> GetInvoice([FromBody] DataResult<InvoiceDto> dto)
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Factura recuperada con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "FacturaElectronica/GetInvoice");
                resultItem = response;

                return resultItem;
            }
            catch (Exception )
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al recuperar la factura, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceFullDataDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceFullDataDto>> GetInvoiceFullData([FromBody] DataResult<InvoiceFullDataDto> dto)
        {
            DataResult<InvoiceFullDataDto> resultItem = new DataResult<InvoiceFullDataDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Factura recuperada con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "FacturaElectronica/GetInvoiceFullData");
                resultItem = response;

                return resultItem;
            }
            catch (Exception )
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al recuperar la factura, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost("FacturaElectronica/SaveInvoice")]
        [ProducesResponseType(typeof(DataResult<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceDto>> SaveInvoice([FromBody] DataResult<InvoiceDto> dto)
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Factura guardada con éxito."
            };
            try
            {
                if (dto.User != null)
                {
                    dto.Data.UserId = dto.User.UserID;
                }
                dto.Data.LastStatusDate = DateTime.Now;
                var response = await _utility.Post(dto, "FacturaElectronica/SaveInvoice");
                resultItem = response;

                return resultItem;
            }
            catch (Exception )
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceEstatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceEstatusDto>> SetInvoiceEstatus([FromBody] DataResult<InvoiceEstatusDto> dto)
        {
            DataResult<InvoiceEstatusDto> resultItem = new DataResult<InvoiceEstatusDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Estatus modificado con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "FacturaElectronica/SetInvoiceEstatus");
                resultItem = response;

                return resultItem;
            }
            catch (Exception )
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceEstatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceEstatusDto>> SetInvoiceLastStatus([FromBody] DataResult<InvoiceEstatusDto> dto)
        {
            DataResult<InvoiceEstatusDto> resultItem = new DataResult<InvoiceEstatusDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Estatus modificado con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "FacturaElectronica/SetInvoiceLastStatus");
                resultItem = response;

                return resultItem;
            }
            catch (Exception )
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceSapDocumentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceSapDocumentDto>> SetInvoiceSapDocument([FromBody] DataResult<InvoiceSapDocumentDto> dto)
        {
            DataResult<InvoiceSapDocumentDto> resultItem = new DataResult<InvoiceSapDocumentDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "SapDocument modificado con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "FacturaElectronica/SetInvoiceSapDocument");
                resultItem = response;

                return resultItem;
            }
            catch (Exception )
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceCxPDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceCxPDto>> SaveInvoiceCxP([FromBody] DataResult<InvoiceCxPDto> dto)
        {
            var result = await SaveInvoiceCxPProcess(dto);

            return result;
        }

        public async Task<DataResult<InvoiceCxPDto>> SaveInvoiceCxPProcess(DataResult<InvoiceCxPDto> dto)
        {
            DataResult<InvoiceCxPDto> resultItem = new DataResult<InvoiceCxPDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "CxP guardado con éxito."
            };
            try
            {
                if (dto.User != null)
                {
                    dto.Data.UserId = dto.User.UserID;
                }
                var response = await _utility.Post(dto, "FacturaElectronica/SaveInvoiceCxP");
                resultItem = response;

                return resultItem;
            }
            catch (Exception )
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceNotaCreditoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceNotaCreditoDto>> SaveInvoiceNotaCredito([FromBody] DataResult<InvoiceNotaCreditoDto> dto)
        {
            var result = await SaveInvoiceNotaCreditoProcess(dto);

            return result;
        }

        public async Task<DataResult<InvoiceNotaCreditoDto>> SaveInvoiceNotaCreditoProcess(DataResult<InvoiceNotaCreditoDto> dto)
        {
            DataResult<InvoiceNotaCreditoDto> resultItem = new DataResult<InvoiceNotaCreditoDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "InvoiceNotaCredito guardado con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "FacturaElectronica/SaveInvoiceNotaCredito");
                resultItem = response;

                return resultItem;
            }
            catch (Exception)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceCxPListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceCxPListDto>> GetInvoiceCxPList([FromBody] DataResult<InvoiceCxPListDto> dto)
        {
            DataResult<InvoiceCxPListDto> resultItem = new DataResult<InvoiceCxPListDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "CxP guardado con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "FacturaElectronica/GetInvoiceCxPList");
                resultItem = response;

                return resultItem;
            }
            catch (Exception)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceValidateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceValidateDto>> ValidateInvoice([FromBody] DataResult<InvoiceValidateDto> dto)
        {
            return await ValidateInvoiceProcess(dto);
        }

        public async Task<DataResult<InvoiceValidateDto>> ValidateInvoiceProcess(DataResult<InvoiceValidateDto> dto)
        {
            DataResult<InvoiceValidateDto> resultItem = new DataResult<InvoiceValidateDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Factura revisada con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "CFDI/ValidateInvoice");
                resultItem = response;

                return resultItem;
            }
            catch (Exception )
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceDto>> GetInvoiceByUUID([FromBody] DataResult<InvoiceDto> dto)
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Factura recuperada con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "FacturaElectronica/GetInvoiceByUUID");
                resultItem = response;

                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceNotaCreditoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceNotaCreditoDto>> GetInvoiceNotaCreditoByUUID([FromBody] DataResult<InvoiceNotaCreditoDto> dto)
        {
            DataResult<InvoiceNotaCreditoDto> resultItem = new DataResult<InvoiceNotaCreditoDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Nota de credito recuperada con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "FacturaElectronica/GetInvoiceNotaCreditoByUUID");
                resultItem = response;

                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceByReceptionDto>> GetInvoiceByReception([FromBody] DataResult<InvoiceByReceptionDto> dto)
        {
            DataResult<InvoiceByReceptionDto> resultItem = new DataResult<InvoiceByReceptionDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Factura recuperada con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "FacturaElectronica/GetInvoiceByReception");
                resultItem = response;

                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<InvoiceNotaCreditoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<InvoiceNotaCreditoByReceptionDto>> GetInvoiceNotaCreditoByReception([FromBody] DataResult<InvoiceNotaCreditoByReceptionDto> dto)
        {
            DataResult<InvoiceNotaCreditoByReceptionDto> resultItem = new DataResult<InvoiceNotaCreditoByReceptionDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Nota de credito recuperada con éxito."
            };
            try
            {
                var response = await _utility.Post(dto, "FacturaElectronica/GetInvoiceNotaCreditoByReception");
                resultItem = response;

                return resultItem;
            }
            catch (Exception ex)
            {
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrió un error al procesar la información, por favor intente de nuevo. En caso de persistir el error, por favor contacte a su administrador.";
                return resultItem;
            }
        }
    }
}
