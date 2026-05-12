using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BERRecepcion.Front.Controllers
{
    //[ValidateUser]
    [AllowAnonymous]
    public class CFDIController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        protected readonly IGenerals _generals;
        protected readonly IRestUtility _rest;
        private readonly IHostEnvironment _env;

        public CFDIController(IConfiguration configuration, IRestUtility utility, IGenerals generals, IHostEnvironment env, IRestUtility rest)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
            _env = env;
            _rest = rest;
        }

        [HttpPost]
        public async Task<IActionResult> SaveInvoice(ComprobanteDto comprobante)
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Proceso exitoso."
            };
            try
            {
                comprobante.User = _generals.User;                
                var response = await _utility.Post<ComprobanteDto>(comprobante, "CFDI/SaveInvoice");
                if (response.existedException)
                {
                    Log.Error(response.exceptionMessage);
                    return Json(new { success = false, message = "Ocurrió un error al enviar la factura, por favor intente más tarde." });
                }
                else
                {
                    comprobante.status = response.status;
                    comprobante.validationErrors = response.validationErrors;
                    return Ok(comprobante);
                }
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, CriticalError = true, isTimeOut = true, Message = "Continuamos procesando tu solicitud, en breve te notificaremos via correo electronico el resultado." });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al enviar la factura, por favor intente más tarde." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveInvoiceMultiple(IEnumerable<ComprobanteDto> comprobantes)
        {
            DataResult<InvoiceDto> resultItem = new DataResult<InvoiceDto>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Proceso exitoso."
            };
            try
            {
                foreach (var comprobante in comprobantes)
                {
                    comprobante.User = _generals.User;
                }
                var response = await _utility.Post<IEnumerable<ComprobanteDto>>(comprobantes, "CFDI/SaveInvoiceMultiple");
                
                return Ok(response);
                
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, CriticalError = true, isTimeOut = true, Message = "Continuamos procesando tu solicitud, en breve te notificaremos via correo electronico el resultado." });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al enviar la factura, por favor intente más tarde." });
            }
        }

    }
}
