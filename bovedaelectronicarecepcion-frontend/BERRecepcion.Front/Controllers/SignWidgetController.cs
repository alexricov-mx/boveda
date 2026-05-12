using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.eSignDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    public class SignWidgetController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IGenerals _generals;
        public SignWidgetController(IConfiguration configuration, IGenerals generals)
        {
            _configuration = configuration;
            _generals = generals;
        }
        
        [HttpPost]
        public async Task<IActionResult> ValidaOCSP(IFormFile cerClientePath)
        {
            try
            {

                CertificadoB64 cert = new CertificadoB64()
                {
                    Certificado = Convert.ToBase64String(_generals.GetBytesFromFile(cerClientePath))
                };

                var client = new RestClient(_configuration["ApiUrl"]);
                var request = new RestRequest("ESign/ValidaCertificado", Method.Post);
                request.AddHeader("ApiKey", _configuration.GetSection("Seguridad:ApiKey").Value);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", cert, ParameterType.RequestBody);
                //request.AddFile("cerClientePath", _generals.GetBytesFromFile(cerClientePath), cerClientePath.FileName, cerClientePath.ContentType);

                var response = await client.ExecuteAsync(request);
                var result = JsonConvert.DeserializeObject<DataResult<string>>(response.Content);
                if (result.Status != System.Net.HttpStatusCode.OK || result.Data == null)
                    return Json(new { success = false, message = "Ocurrió un error al consultar el estado del certificado, por favor intente nuevamente." });
                return Json(new { success = true, isValid = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al validar el certificado" });
            }
        }
    }
}
