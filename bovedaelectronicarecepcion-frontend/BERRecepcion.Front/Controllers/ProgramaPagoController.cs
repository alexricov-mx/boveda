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
    public class ProgramaPagoController : Controller
    {
        #region Servicios
        private static readonly string GetPayments = "PaymentSchedule/GetPayments";
        private static readonly string PostFirmaPP = "PaymentSchedule/FirmarAsync";
        private static readonly string PostValidaOCreaUsuarios = "ESign/ValidaOCreaUsuarios";
        private static readonly string PostCompletaFirmaPP = "PaymentSchedule/CompletaFirmaAsync";
        #endregion

        #region Paginado
        private static readonly string PageSize = "pageSize";
        private static readonly string PageNumber = "pageNum";
        private static readonly string Search = "search";
        private static readonly string TokenPP = "Token";
        #endregion

        #region Mensajes
        private static readonly string FirmaExitosa = "Firma exitosa";
        private static readonly string ErrorProgramaPago = "Ocurrió un error al cargar los programas de pago, por favor intente más tarde";
        private static readonly string ErrorFirma = "Ocurrió un error al firmar, por favor intente mas tarde";
        private static readonly string ErrorEnvioCorreo = "La firma se completó satisfactoriamente, pero ocurrió un error al realizar el envío del correo electrónico.";
        private static readonly string FirmaCompleta = "La firma se completó de manera satisfactoria.";
        #endregion

        #region Identificadores
        //private static readonly string DoctoBE = "DocumentoBEId";
        private static readonly string PDFCheck = "PDFCheckDisabled:ProgramaPago";
        private readonly IConfiguration _configuration;
        private readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        private readonly IHostEnvironment _env;
        private readonly static string TipoDocumentoPP = "PP";
        private static readonly int FirmateUnico = 1;
        #endregion
       
        public ProgramaPagoController(IConfiguration configuration, IRestUtility utility, IGenerals generals, IHostEnvironment env)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
            _env = env;
        }
        [RoleFilter(Roles: "StatisticsPaymentSchedule")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ProgramaPagoCard(int pageNum = 1, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration["Paginacion:ProgramaPago"]);                
                param.Add(new CustomHttpParameter(PageSize, pageSize));
                param.Add(new CustomHttpParameter(PageNumber, pageNum));
                param.Add(new CustomHttpParameter(Search, search));
                param.Add(new CustomHttpParameter(TokenPP, _generals.User.Token));

                var payments = await _utility.GetItem<DataResult<IEnumerable<PaymentScheduleDto>>>(GetPayments, param);
                payments.Pager = new Pager(payments.Pager.TotalItems, pageNum, pageSize);
                ViewBag.PDFCheckDisabled = Convert.ToBoolean(_configuration[PDFCheck]);
                ViewBag.Search = search;
                return PartialView("_ProgramaPagoCard", payments);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorProgramaPago });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Firmar([FromBody] PaymentScheduleDto model)
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
                    paquete = new CrearPaquete()
                    {
                        Titulo = $"{TipoDocumentoPP.ToUpper()} {model.ProgramaPago_Id.ToString()}",
                        Descripcion = model.ProgramaPago_Id.ToString(),
                        EnProcesoFirma = true,
                        TotalFirmantes = FirmateUnico,

                        Documentos = new List<Documento> {
                            new Documento
                            {
                                Descripcion = $"{TipoDocumentoPP.ToUpper()} {model.ProgramaPago_Id.ToString()}",
                                CodigoTipoDocumento = TipoDocumentoPP.ToUpper(),
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
                var _result = await _utility.Post(dataResult, PostFirmaPP);
                if (_result.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = _result.Message });
                return Json(new { success = true, message = FirmaExitosa, _result.Data });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorFirma });
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

                var completafirma = await _utility.Post(externosCompletaFirma, PostCompletaFirmaPP);
                if (completafirma.Status == System.Net.HttpStatusCode.BadRequest)
                    return Json(new { success = false, message = completafirma.Message });
                if (completafirma.Status == System.Net.HttpStatusCode.Conflict)
                    return Json(new { success = true, info = true, message = ErrorEnvioCorreo });
                return Json(new { success = true, message = FirmaCompleta });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorFirma });
            }
        }
    }
}
