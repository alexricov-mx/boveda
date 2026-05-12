using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BERRecepcion.Front.Models.IntegracionEFirma;
using static BERRecepcion.Front.Models.CrearPaquete;
using System.Linq;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class AnaliticoPagoController : Controller
    {
        //Variables de trabajo
        #region Firmantes
        private static readonly int FirmateUnico = 1;
        #endregion

        #region Servicios
        private static readonly string GetAnaliticoPago = "AnaliticoPago/ConsultaAPGetAllAsync";
        private static readonly string PostValidaOCreaUsuarios = "ESign/ValidaOCreaUsuarios";
        private static readonly string PostFirmaUnoAP = "AnaliticoPago/FirmaUnoAsync";
        private static readonly string PostCompletaFirmaAP = "AnaliticoPago/CompletaFirmaAsync";
        #endregion

        #region Paginado
        private static readonly string PageSize = "pageSize";
        private static readonly string PageNumber = "pageNum";
        private static readonly string Search = "search";
        private static readonly string Ficha = "ficha";
        #endregion

        #region Mensajes
        private static readonly string FirmaExitosa = "Firma exitosa";
        private static readonly string ErrorCargaAP = "Ocurrió un error al cargar el analitico de pago, por favor intente mas tarde";
        private static readonly string ErrorFirmado = "Ocurrió un error al firmar, por favor intente más tarde";
        private static readonly string ErrorEnvioCorreo = "La firma se completó satisfactoriamente, pero ocurrió un error al realizar el envío del correo electrónico.";
        private static readonly string FirmaCompleta = "La firma se completó de manera satisfactoria.";
        #endregion

        #region Identificadores
        protected readonly IConfiguration _configuration;
        private readonly ILogger<AnaliticoPagoController> _logger;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        private static readonly string TipoDocumentoAP = "AP";
        private readonly static string SeccionAP = "AnaliticoPago";
        private readonly static string AccionAP = "Firmar";
        private readonly static string DescripcionAP = "Firma de Analitico de Pago ";
        #endregion

        //Variables de trabajo

        public AnaliticoPagoController(ILogger<AnaliticoPagoController> logger, IRestUtility utility, IGenerals generals, IConfiguration configuration)
        {
            _logger = logger;
            _utility = utility;
            _generals = generals;
            _configuration = configuration;
        }
        [RoleFilter(Roles: "AnalyticalPayment")]
        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        public ActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetAP(string search = "", int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:AnaliticoPago").Value);
                if (pageSize == 0) pageSize = 10;
                param.Add(new CustomHttpParameter(Ficha, _generals.User.Token));
                param.Add(new CustomHttpParameter(Search, search));
                param.Add(new CustomHttpParameter(PageSize, pageSize));
                param.Add(new CustomHttpParameter(PageNumber, pageNum));

                var ap = await _utility.GetItem<DataResult<IEnumerable<AnaliticoPagoDto>>>(GetAnaliticoPago, param);
                if (ap.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = ErrorCargaAP });
                ap.Pager = new Pager(ap.Pager.TotalItems, pageNum, pageSize);
                return PartialView("_AnaliticoPagoTabla", ap);
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = ErrorCargaAP });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Firmar([FromBody] AnaliticoPagoDto model)
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
                var documentoResult = new DataResult<Externos2Dto>();
                dataResult.Data = new Externos2Dto
                {
                    usuario = responseUsuario.Data.usuario,
                    documentoBEId = model.AnaliticoPagoID,
                    paquete = new CrearPaquete()
                    {
                        Titulo = $"{TipoDocumentoAP.ToUpper()} {model.IdAnalitico.ToString()}",
                        Descripcion = model.IdAnalitico.ToString(),
                        EnProcesoFirma = true,
                        TotalFirmantes = FirmateUnico,

                        Documentos = new List<Documento> {
                            new Documento
                            {
                                Descripcion = $"{TipoDocumentoAP.ToUpper()} {model.IdAnalitico.ToString()}",
                                CodigoTipoDocumento = TipoDocumentoAP.ToUpper(),
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

                if (string.IsNullOrWhiteSpace(model.FunctionarySignDate.ToString()))
                    documentoResult = await _utility.Post(dataResult, PostFirmaUnoAP);
                
                if(documentoResult.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = documentoResult.Message });

                return Json(new { success = true, message = FirmaExitosa, documentoResult.Data });
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

                var Completafirma = await _utility.Post(externosCompletaFirma, PostCompletaFirmaAP);
                if (Completafirma.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = Completafirma.Message });
                if (Completafirma.Status == System.Net.HttpStatusCode.Conflict)
                    return Json(new { success = true, info = true, message = ErrorEnvioCorreo });
                return Json(new { success = true, message = FirmaCompleta });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorFirmado });
            }
        }

    }
}
