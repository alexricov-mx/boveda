using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Models.IntegracionEFirma;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BERRecepcion.Front.Models.CrearPaquete;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class EstimacionObraController : Controller
    {
        #region Servicios
        private static readonly string GetSOEInterno = "SOEstimation/GetSOEInternoAsync";
        private static readonly string GetSOEProveedor = "SOEstimation/GetSOEProveedorAsync";
        private static readonly string GetDocumentoFirmadoEO = "DocumentoFirmado/GetDocumentoFirmadoAsync";
        private static readonly string GetListaFiltroCopadesEO = "Copade/GetListaFiltroCopadesAsync";
        private static readonly string PostValidaOCreaUsuarios = "ESign/ValidaOCreaUsuarios";
        private static readonly string PostFirmaUnoEO = "SOEstimation/FirmaUnoAsync";
        private static readonly string PostFirmaDosEO = "SOEstimation/FirmaDosAsync";
        private static readonly string PostCompletaFirmaEO = "SOEstimation/CompletaFirmaAsync";
        #endregion

        #region Paginado
        private static readonly string PageSize = "pageSize";
        private static readonly string PageNumber = "pageNum";
        private static readonly string UserTypEO = "UserTypeP";
        private static readonly string TokenEO = "Token";
        private static readonly string FiltroEO = "Filtro";
        private static readonly string CreditorNumberEO = "CreditorNumber";

        #endregion

        #region Mensajes
        private static readonly string FirmaExitosa = "Firma exitosa";
        private static readonly string ErrorFirmado = "Ocurrió un error al firmar, por favor intente más tarde";
        private static readonly string ErrorCargaEstimacionObra = "Ocurrió un error al cargar la estimación de obra, por favor intente mas tarde";
        private static readonly string DescripcionEO = "Firma de Estimación de obra ";
        #endregion

        #region Identificadores
        private static readonly string DoctoBE = "DocumentoBEId";
        private static readonly string PDFCheck = "PDFCheckDisabled:EstimacionObra";
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        protected readonly IGenerals _generals;
        private static readonly int FirmateUnico = 1;
        private static readonly int DosFirmates = 2;
        private readonly static string TipoDocumento = "ES";
        private readonly static string SeccionEO = "EstimacionObra";
        private readonly static string AccionEO = "Firmar";

        #endregion

        public EstimacionObraController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        [RoleFilter(Roles: "ReceptionSignSOEstimations")]
        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF,UserTypeP")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EstimacionObraTable(int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:EstimacionObra").Value);
                param.Add(new CustomHttpParameter(PageSize, pageSize));
                param.Add(new CustomHttpParameter(PageNumber, pageNum));

                var estimacionObra = new DataResult<IEnumerable<SOEstimationDto>>();
                if (_generals.User.UserType != UserTypEO)                    
                {
                    param.Add(new CustomHttpParameter(TokenEO, _generals.User.Token));
                   
                    estimacionObra = await _utility.GetItem<DataResult<IEnumerable<SOEstimationDto>>>(GetSOEInterno, param);
                }
                else
                {
                    param.Add(new CustomHttpParameter(CreditorNumberEO, _generals.User.CreditorNumber));
                    estimacionObra = await _utility.GetItem<DataResult<IEnumerable<SOEstimationDto>>>(GetSOEProveedor, param);
                }
                
                if (estimacionObra.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    estimacionObra.Pager = new Pager(estimacionObra?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={estimacionObra.Pager.TotalItems}");
                    estimacionObra.Pager = new Pager(estimacionObra.Pager.TotalItems, pageNum, pageSize);
                }
                ViewBag.PDFCheckDisabled = Convert.ToBoolean(_configuration[PDFCheck]);
                return PartialView("_EstimacionObraTable", estimacionObra);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = ErrorCargaEstimacionObra });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Firmar([FromBody] SOEstimationDto model)
        {
            try
            {
                DataResult<UsuarioDto> responseExternos = new DataResult<UsuarioDto>
                {
                    Data = _generals.Usuario
                };
                DataResult<UsuarioDto> responseUsuario = await _utility.Post(responseExternos, PostValidaOCreaUsuarios);

                if (responseUsuario.Status != System.Net.HttpStatusCode.OK || responseUsuario.Data == null)
                    return Json(new { success = false, message = responseUsuario.Message });

                ////Peticiones para firma
                DataResult<Externos2Dto> dataResult = new DataResult<Externos2Dto>() { User = _generals.User };
                DataResult<Externos2Dto> documentoResult = new DataResult<Externos2Dto>();
                dataResult.Data = new Externos2Dto
                {
                    sOEstimation = model,
                    usuario = responseUsuario.Data,
                    usuarioBEId = _generals.User.UserID,
                    documentoBEId = model.EstimacionID,
                    paquete = new CrearPaquete()
                    {
                        Titulo = $"{TipoDocumento.ToUpper()} {model.SapOrder.ToString()}",
                        Descripcion = model.SapOrder.ToString(),
                        EnProcesoFirma = true,
                        TotalFirmantes = DosFirmates,

                        Documentos = new List<Documento> {
                            new Documento
                            {
                                Descripcion = $"{TipoDocumento.ToUpper()} {model.SapOrder.ToString()}",
                                CodigoTipoDocumento = TipoDocumento.ToUpper(),
                            }
                        },
                        Firmantes = new List<Firmante>
                        {
                            new Firmante
                            {
                                IdUsuario = responseUsuario.Data.Id,
                                Figura = string.IsNullOrWhiteSpace(model.ProviderSignDate.ToString()) ?
                                                                    FirmateUnico.ToString() : DosFirmates.ToString()
                            }
                        }
                    }
                };

                if (string.IsNullOrWhiteSpace(model.ProviderSignDate.ToString()))
                    documentoResult = await _utility.Post(dataResult, PostFirmaUnoEO);
                else
                {
                    List<CustomHttpParameter> param = new List<CustomHttpParameter>();
                    param.Add(new CustomHttpParameter(DoctoBE, model.EstimacionID));
                    DataResult<DocumentoFirmadoDto> documentoFirmado = await _utility.GetItem<DataResult<DocumentoFirmadoDto>>(GetDocumentoFirmadoEO, param);
                    if (documentoFirmado.Status != System.Net.HttpStatusCode.OK)
                        return Json(new { success = false, message = documentoFirmado.Message });

                    dataResult.Data.agregarFirmante = new AgregaFirmante();
                    dataResult.Data.agregarFirmante.Firmantes = new List<Firmante>() {
                        new Firmante ()
                        {
                            IdUsuario = responseUsuario.Data.Id,
                            Figura = DosFirmates.ToString()
                        }
                    };

                    documentoResult = await _utility.Post(dataResult, PostFirmaDosEO);

                }
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
                var Completafirma = await _utility.Post(externosCompletaFirma, PostCompletaFirmaEO);
                if (Completafirma.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = Completafirma.Message });

                return Json(new { success = true, message = FirmaExitosa });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorFirmado });
            }
        }

        [HttpGet]
        public async Task<IActionResult> obtenerFiltroEStimationTable(string filtro, int pageNum = 1)
        {
            var param = new List<CustomHttpParameter>();
            var CopadeSeguimiento = new DataResult<IEnumerable<CopadeDto>>();

            int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
            param.Add(new CustomHttpParameter(PageSize, pageSize));
            param.Add(new CustomHttpParameter(PageNumber, pageNum));

            if (_generals.User.UserType != UserTypEO)
            {
                param.Add(new CustomHttpParameter(TokenEO, _generals.User.Token));
                CopadeSeguimiento = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>(GetListaFiltroCopadesEO, param);
            }
            else
            {
                param.Add(new CustomHttpParameter(FiltroEO, filtro));
                CopadeSeguimiento = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>(GetListaFiltroCopadesEO, param);

            }

            CopadeSeguimiento.Pager = new Pager(0, pageNum, pageSize);
            return PartialView("GenerarTablaCopade", CopadeSeguimiento);
        }
    }
}
