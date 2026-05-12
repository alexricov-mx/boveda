using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    [AllowAnonymous]
    public class ESignController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        public ESignController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        //Evidencia de firma
        [HttpGet]
        public async Task<IActionResult> ConsultaDocumentoFirma(Guid DocumentoBEId, int Orden)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("DocumentoBEId", DocumentoBEId));
                param.Add(new CustomHttpParameter("Orden", Orden));
                var documentoFirmado = await _utility.GetItem<DataResult<DocumentoFirmadoDto>>("DocumentoFirmado/GetDocumentoFirmadoAsync", param);
                if (documentoFirmado.Status != System.Net.HttpStatusCode.OK || documentoFirmado.Data == null)
                    return Json(new { success = false, message = documentoFirmado.Message });
                param.Clear();
                param.Add(new CustomHttpParameter("PaqueteId", documentoFirmado.Data.paqueteId));
                param.Add(new CustomHttpParameter("DocumentoId", documentoFirmado.Data.documentoId));
                param.Add(new CustomHttpParameter("UsuarioId", documentoFirmado.Data.usuarioId));
                var detalleFirma = await _utility.GetItem<DataResult<ExternosDto>>("ESign/ConsultaDocumentoFirma", param);
                if (detalleFirma.Status != System.Net.HttpStatusCode.OK || detalleFirma.Data == null)
                    return Json(new { success = false, message = documentoFirmado.Message });
                ViewBag.QRFirmaDetalle = _generals.GenerateQRCode(detalleFirma.Data.UrlQR);
                return PartialView("_ConsultaDocumentoFirma", detalleFirma);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Ocurrió un error al consultar el detalle de la firma, favor de intentarlo nuevamente" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> PrefacturaConsultar(string filtro)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                var CopadeSeguimiento = new DataResult<IEnumerable<CopadeDto>>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", 1));
                param.Add(new CustomHttpParameter("Filtro", filtro));
                param.Add(new CustomHttpParameter("UserID", new Guid("A0AE9221-FBB4-EB11-A2E7-005056ADB859")));/*_generals.User.UserID*/
                CopadeSeguimiento = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>("Copade/GetListaFiltroCopadesAsync", param);

                return PartialView("_Prefactura", CopadeSeguimiento);

            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Ocurrió un error al consultar, favor de intentarlo nuevamente" });
            }
        }
    }
}
