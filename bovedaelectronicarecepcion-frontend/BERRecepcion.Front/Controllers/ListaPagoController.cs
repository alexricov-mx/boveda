using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BERRecepcion.Front.Models.IntegracionEFirma;
using static BERRecepcion.Front.Models.CrearPaquete;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class ListaPagoController : Controller
    {
        //Variables de trabajo
        #region Firmantes
        private static readonly int FirmateUnico = 1;
        #endregion

        #region Servicios
        private static readonly string GetPayments = "PaymentList/GetPayments";
        private static readonly string PostValidaOCreaUsuarios = "ESign/ValidaOCreaUsuarios";
        private static readonly string PostFirmaLP = "PaymentList/FirmarAsync";
        private static readonly string PostCompletaFirmaLP = "PaymentList/CompletaFirmaAsync";
        #endregion

        #region Paginado
        private static readonly string PageSize = "pageSize";
        private static readonly string PageNumber = "pageNum";
        private static readonly string Search = "search";
        private static readonly string Token = "Token";
        #endregion

        #region Mensajes
        private static readonly string FirmaExitosa = "Firma exitosa";
        private static readonly string ErrorCargaLP = "Ocurrió un error al cargar las listas de pago, por favor intente mas tarde";
        private static readonly string ErrorFirmado = "Ocurrió un error al firmar, por favor intente más tarde";
        private static readonly string ErrorEnvioCorreo = "La firma se completó satisfactoriamente, pero ocurrió un error al realizar el envío del correo electrónico.";
        private static readonly string FirmaCompleta = "La firma se completó de manera satisfactoria.";
        #endregion

        #region Identificadores
        private readonly IConfiguration _configuration;
        private readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        private readonly IHostEnvironment _env;
        private readonly static string ListaPago = "PDFCheckDisabled:ListaPago";   
        private static readonly string TipoDocumentoLP = "LP";
        #endregion

        //Variables de trabajo
       
        public ListaPagoController(IConfiguration configuration, IRestUtility utility, IGenerals generals, IHostEnvironment env)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
            _env = env;
        }
        [RoleFilter(Roles: "StatisticsPaymentList")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaPagoCard(int pageNum = 1, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration["Paginacion:ListaPago"]);                
                param.Add(new CustomHttpParameter(PageSize, pageSize));
                param.Add(new CustomHttpParameter(PageNumber, pageNum));
                param.Add(new CustomHttpParameter(Search, search));
                param.Add(new CustomHttpParameter(Token, _generals.User.Token));

                var payments = await _utility.GetItem<DataResult<IEnumerable<PaymentListDto>>>(GetPayments, param);
                payments.Pager = new Pager(payments.Pager.TotalItems, pageNum, pageSize);
                ViewBag.PDFCheckDisabled = Convert.ToBoolean(_configuration[ListaPago]);
                ViewBag.Search = search;
                return PartialView("_ListaPagoCard", payments);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorCargaLP });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Firmar([FromBody] PaymentListDto model)
        {
            try
            {
                //Validar usuario
                DataResult<Externos2Dto> responseExternos = new DataResult<Externos2Dto> { Data = new Externos2Dto { usuario = _generals.Usuario } };
                var responseUsuario = await _utility.Post(responseExternos, PostValidaOCreaUsuarios);

                if (responseUsuario.Status != System.Net.HttpStatusCode.OK || responseUsuario.Data == null)
                    return Json(new { success = false, message = responseUsuario.Message });

                var dataResult = new DataResult<Externos2Dto>() { User = _generals.User };
                dataResult.Data = new Externos2Dto
                {
                    usuario = responseUsuario.Data.usuario,
                    //documentoBEId = model.,
                    paquete = new CrearPaquete()
                    {
                        Titulo = $"{TipoDocumentoLP.ToUpper()} {model.ListaPago_Id.ToString()}",
                        Descripcion = model.ListaPago_Id.ToString(),
                        EnProcesoFirma = true,
                        TotalFirmantes = FirmateUnico,

                        Documentos = new List<Documento> {
                            new Documento
                            {
                                Descripcion = $"{TipoDocumentoLP.ToUpper()} {model.ListaPago_Id.ToString()}",
                                CodigoTipoDocumento = TipoDocumentoLP.ToUpper(),
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
                var _result = await _utility.Post(dataResult, PostFirmaLP);
                if (_result.Status == System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = _result.Message });
                return Json(new { success = true, message = FirmaExitosa, _result.Data });
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

                var completafirma = await _utility.Post(externosCompletaFirma, PostCompletaFirmaLP);
                if (completafirma.Status == System.Net.HttpStatusCode.BadRequest)
                    return Json(new { success = false, message = completafirma.Message });
                if (completafirma.Status == System.Net.HttpStatusCode.Conflict)
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
