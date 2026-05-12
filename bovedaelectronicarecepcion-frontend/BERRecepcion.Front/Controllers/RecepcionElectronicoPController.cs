using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Views.RecepcionElectronicoP
{
    public class RecepcionElectronicoPController : Controller
    {
        private readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;

        public RecepcionElectronicoPController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        public IActionResult Index()
        {
            return View(_generals.User);
        }
        public async Task<IActionResult> ExisteREPAsync(IFormFile factura)
        {
            if (factura == null)
                return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
            var _comprobante = await _generals.GetComprobante(factura, esRecepcionEP: true);
            if (_comprobante.Status != System.Net.HttpStatusCode.OK || _comprobante.Data == null)
                return Json(new { success = false, message = _comprobante.Message });

            if (!_comprobante.Data.ComprobanteBE.Emisor.Rfc.Equals(_generals.User.CreditorRFC) && !_comprobante.Data.ComprobanteBE.Receptor.Rfc.Equals(_generals.User.Organisms.FirstOrDefault().Rfc))
                return Json(new { success = false, message = "El complemento seleccionado no coincide con el RFC del Emisor, favor de verificar." });


            var recepcionEP = new RecepcionElectronicaPViewModel
            {
                comprobante = _comprobante.Data.ComprobanteBE,
                Archivo = factura
            };


            foreach (var item in _comprobante.Data.ComprobanteBE.Complemento.Pagos.Pago.DoctoRelacionado)
            {

                Guid newGuid = Guid.Parse(item.IdDocumento);
                var repDto = new InvoiceEstatusDto
                {
                    InvoiceId = newGuid
                };

                var result = await _utility.GetItem<DataResult<bool>>("RecepcionEP/GetValidaInvoiceDocAsync/" + newGuid);
               
                if (result.Data == true)
                {
                    item.ExisteEnBD = true;
                }

            }


            return PartialView("_TextRecepcionEP", recepcionEP);
        }

        [HttpPost]
        public async Task<IActionResult> Validar(IFormFile factura, string addenda = "")
        {
            if (factura == null)
                return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
            var _comprobante = await _generals.GetComprobante(factura, esRecepcionEP: true);
            if (_comprobante.Status != System.Net.HttpStatusCode.OK)
                return Json(new { success = false, message = _comprobante.Message });
            
            if (_comprobante.Status != System.Net.HttpStatusCode.OK || _comprobante.Data == null)
                return Json(new { success = false, message = _comprobante.Message });

            var recepcionEP = new RecepcionElectronicaPViewModel
            {
                comprobante = _comprobante.Data.ComprobanteBE,
                Archivo = factura
            };

            var test = await _generals.ReadFileAsync(factura);
            var repDto = new RecepcionElectronicaPDto
            {
                comprobante = _comprobante.Data.ComprobanteBE,
                xml = await _generals.ReadFileAsync(factura)
            };


             DataResult<RecepcionElectronicaPDto> resultItem = new DataResult<RecepcionElectronicaPDto>()
             {
                 Status = System.Net.HttpStatusCode.OK,
                 Message = "Factura recuperada con éxito.",
                 Data = repDto

             };

            

            return PartialView("_TextRecepcionEP", recepcionEP);
        }

        [HttpPost]
        public async Task<IActionResult> EnviarFactura(IFormFile factura, IEnumerable<IFormFile> notasCredito, string addenda = "")
        {
            if (factura == null)
                return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
            var _comprobante = await _generals.GetComprobante(factura, esRecepcionEP: true);
            if (_comprobante.Status != System.Net.HttpStatusCode.OK)
                return Json(new { success = false, message = _comprobante.Message });
            ComprobanteBE comprobante = _comprobante.Data.ComprobanteBE;
            //Validar si el comprobante contiene addenda
           
            try
            {


                var repDto = new RecepcionElectronicaPDto
                {
                    comprobante = _comprobante.Data.ComprobanteBE,
                    xml = await _generals.ReadFileAsync(factura),
                    UUid = _comprobante.Data.ComprobanteBE.Complemento.TimbreFiscalDigital.UUID
                    
                };

                var dataResult = new DataResult<RecepcionElectronicaPDto>
                {
                    Data = repDto,
                    User = _generals.User
                    //User = new UsersDto {UserID = Guid.Parse("8B079B25-5528-4737-BB00-295858144823") } 
                };

                

                var response = await _utility.Post<DataResult<RecepcionElectronicaPDto>>(dataResult, "RecepcionEP/InsertaPagosDocAsync");

                if (response.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = response.Message});

                return Json(new { success = true, message = "Datos enviados Correctamente." });
            }
             catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al enviar el complemento, por favor intente más tarde." });
            }
        }
    }
}
