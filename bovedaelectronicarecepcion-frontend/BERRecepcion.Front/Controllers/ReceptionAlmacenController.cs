using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using BERRecepcion.Front.Models.IntegracionEFirma;
using static BERRecepcion.Front.Models.CrearPaquete;
using SpreadsheetLight;
// Ya quedo

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class ReceptionAlmacenController : Controller
    {
        #region Servicios
        private static readonly string GetReceptionRA = "ReceptionAlmacen/GetReceptionAsync";
        private static readonly string PostValidaOCreaUsuarios = "ESign/ValidaOCreaUsuarios";
        private static readonly string PostFirmaRA = "ReceptionAlmacen/FirmaAsync";
        private static readonly string PostCompletaFirmaRA = "ReceptionAlmacen/CompletaFirmaAsync";
        private static readonly string GetDocumentoFirmadoRA = "ReceptionAlmacen/GetReceptionPDFAsync";
        #endregion

        #region Paginado
        private static readonly string PageSize = "pageSize";
        private static readonly string PageNumber = "pageNum";
        private static readonly string Search = "search";
        private static readonly string TokenRA = "Token";
        #endregion

        #region Mensajes
        private static readonly string FirmaExitosa = "Firma exitosa";
        private static readonly string ErrorFirmado = "Ocurrió un error al firmar, por favor intente más tarde";
        private static readonly string ErrorRecepcion = "Ocurrió un error al cargar la información, por favor intente más tarde.";
        private static readonly string FirmaCompleta = "La firma se completó de manera satisfactoria.";
        private static readonly string DescripcionRA = "Firma de Recepción en Almacen ";
        private static readonly string ErrorConsultaDocumentoRA = "Ocurrió un error al consultar el documento, favor de intentar más tarde.";
        #endregion

        #region Identificadores
        private static readonly string PDFCheckRA = "PDFCheckDisabled:ReceptionAlmacen";
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        protected readonly IGenerals _generals;
        private readonly IHostEnvironment _env;
        private readonly static string TipoDocumento = "RE";
        private readonly static string SeccionRA = "RecepcionAlmacen";
        private readonly static string AccionRA = "Firmar";
        private static readonly int FirmateUnico = 1;
        #endregion

        public ReceptionAlmacenController(IConfiguration configuration, IRestUtility utility, IGenerals generals, IHostEnvironment env)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
            _env = env;
        }
        [RoleFilter(Roles: "ReceptionSignReception")]
        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ReceptionCard(int pageNum = 1, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:ReceptionAlmacen").Value);
                param.Add(new CustomHttpParameter(PageSize, pageSize));
                param.Add(new CustomHttpParameter(PageNumber, pageNum));
                param.Add(new CustomHttpParameter(Search, search));
                param.Add(new CustomHttpParameter(TokenRA, _generals.User.Token));
                var reception = await _utility.GetItem<DataResult<IEnumerable<ReceptionDto>>>(GetReceptionRA, param);
                reception.Pager = new Pager(reception.Pager.TotalItems, pageNum, pageSize);
                ViewBag.PDFCheckDisabled = Convert.ToBoolean(_configuration[PDFCheckRA]);
                ViewBag.Search = search;
                return PartialView("_ReceptionCard", reception);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorRecepcion });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Firmar([FromBody]ReceptionDto model)
        {
            try
            {
                //Validar usuario
                DataResult<Externos2Dto> responseExternos = new DataResult<Externos2Dto> { Data = new Externos2Dto { usuario = _generals.Usuario } };
                var responseUsuario = await _utility.Post(responseExternos, PostValidaOCreaUsuarios);

                if (responseUsuario.Status != System.Net.HttpStatusCode.OK || responseUsuario.Data == null)
                    return Json(new { success = false, message = responseUsuario.Message });

                //Peticiones para firma
                var dataResult = new DataResult<Externos2Dto>() { User = _generals.User };
                dataResult.Data = new Externos2Dto
                {
                    reception = model,
                    usuario = responseUsuario.Data.usuario,
                    documentoBEId = model.ReceptionID,
                    paquete = new CrearPaquete()
                    {
                        Titulo = $"{TipoDocumento.ToUpper()}_{model.Contract.ToString()}_{model.SapOrder.ToString()}_{model.SiafOrder.ToString()}_{model.Reception.ToString()}",
                        Descripcion = model.Contract.ToString() + "_" + model.SapOrder.ToString() + "_" + 
                                      model.SiafOrder.ToString() + "_" +  model.Reception.ToString(),
                        EnProcesoFirma = true,
                        TotalFirmantes = FirmateUnico,
                        Documentos = new List<Documento> {
                            new Documento
                            {
                                Descripcion = $"{TipoDocumento.ToUpper()}_{model.Contract.ToString()}_{model.SapOrder.ToString()}_{model.SiafOrder.ToString()}_{model.Reception.ToString()}",
                                CodigoTipoDocumento = TipoDocumento.ToUpper(),
                            }
                        },
                        Firmantes = new List<Firmante>
                        {
                            new Firmante
                            {
                                IdUsuario = responseUsuario.Data.usuario.Id,
                                Figura = FirmateUnico.ToString()
                            }
                        }
                    }
                };
                var result = await _utility.Post(dataResult, PostFirmaRA);
                return Json(new { success = true, message = FirmaExitosa, dataResult.Data });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorFirmado });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CompletarFirma([FromBody] Externos2Dto externos)
        {
            try
            {
                DataResult<Externos2Dto> responseExternos = new DataResult<Externos2Dto> { Data = new Externos2Dto { usuario = _generals.Usuario } };
                DataResult<Externos2Dto> externosCompletaFirma = new DataResult<Externos2Dto>();
                externosCompletaFirma.Data = externos;
                externosCompletaFirma.Data.usuarioBEId = _generals.User.UserID;
                externosCompletaFirma.Data.usuario = externos.usuario;
                externosCompletaFirma.User = _generals.User;
                externosCompletaFirma.Data.firmaPaquete = new FirmarPaquete
                {
                    IdPaquete = externos.paqueteResult.IdPaquete,
                    IdCorrelacion = externos.paqueteResult.IdCorrelacion,
                    IdUsuarioFirmante = externos.usuario.Id,
                    FiguraFirmante = externos.paqueteResult.Firmantes.FirstOrDefault().Figura,
                    CertificadoFirmanteBase64 = externos.Certificado,
                    Firmas = new List<Firma>()
                    {
                        new Firma
                        {
                            IdDocumento = externos.paqueteResult.Documentos.FirstOrDefault().IdDocumento,
                            Hash = externos.hashFirma,
                            IdCorrelacion = externos.paqueteResult.IdCorrelacion
                        }
                    }
                };

                var Completafirma = await _utility.Post(externosCompletaFirma, PostCompletaFirmaRA);
                if (Completafirma.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = Completafirma.Message });

                return Json(new { success = true, message = FirmaCompleta });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorFirmado });
            }
        }
        public async Task<IActionResult> GetDocumentoPDF(Guid ReceptionId)
        {
            var param = new List<CustomHttpParameter>();
            param.Add(new CustomHttpParameter("ReceptionId", ReceptionId));
            
            try
            {
                var result = await _utility.GetItem<DataResult<ArchivoPDFDto>>(GetDocumentoFirmadoRA, param);
                if (result.Status == System.Net.HttpStatusCode.OK)
                    return Json(new { success = true, file = result.Data.ARCHIVO });
                return Json(new { success = false, message = ErrorConsultaDocumentoRA });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorConsultaDocumentoRA });
            }
        }
    }
}
