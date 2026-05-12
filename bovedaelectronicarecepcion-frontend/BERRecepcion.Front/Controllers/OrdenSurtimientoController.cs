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
using Microsoft.AspNetCore.Authorization;

namespace BERRecepcion.Front.Controllers
{
    //[ValidateUser]
    [Authorize]
    public class OrdenSurtimientoController : Controller
    {
        #region Servicios
        private static readonly string GetListOSInterno = "SupplyOrders/GetOSInternoAsync";
        private static readonly string GetListOSProveedor = "SupplyOrders/GetOSProveedorAsync";
        private static readonly string GetDocumentoFirmadoOS = "DocumentoFirmado/GetDocumentoFirmadoAsync";
        private static readonly string PostFirmaUnoOS = "SupplyOrders/FirmaUnoAsync";
        private static readonly string PostFirmaDosOS = "SupplyOrders/FirmaDosAsync";
        private static readonly string PostValidaOCreaUsuarios = "ESign/ValidaOCreaUsuarios";
        private static readonly string PostCompletaFirmaOS = "SupplyOrders/CompletaFirmaAsync";
        #endregion

        #region Paginado
        private static readonly string PageSize = "pageSize";
        private static readonly string PageNumber = "pageNum";
        private static readonly string Search = "search";
        private static readonly string UserTypeOS = "UserTypeP";
        private static readonly string TokenOS = "Token";
        private static readonly string CreditorNumberOS = "CreditorNumber";
        #endregion

        #region Mensajes
        private static readonly string FirmaExitosa = "Firma exitosa";
        private static readonly string ErrorFirmado = "Ocurrió un error al firmar, por favor intente más tarde";
        private static readonly string ErrorOrdenSurtimientoCard = "Ocurrió un error al cargar las órdenes de surtimiento, por favor intente mas tarde";
        private static readonly string ErrorEnvioCorreo = "La firma se completó satisfactoriamente, pero ocurrió un error al realizar el envío del correo electrónico.";
        private static readonly string FirmaCompleta = "La firma se completó de manera satisfactoria.";
        private static readonly string DescripcionOS = "Firma de la órden de surtimiento ";
        #endregion

        #region Identificadores
        private static readonly string DoctoBE = "DocumentoBEId";
        private static readonly string PDFCheck = "PDFCheckDisabled:OrdenSurtimiento";
        private readonly IConfiguration _configuration;
        private readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        private readonly IHostEnvironment _env;
        private readonly static string TipoDocumentoOS = "OS";
        private readonly static string SeccionOS = "OrdenSurtimiento";
        private readonly static string AccionOS = "Firmar";
        private static readonly int FirmateUnico = 1;
        #endregion

        public OrdenSurtimientoController(IConfiguration configuration, IRestUtility utility, IGenerals generals, IHostEnvironment env)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
            _env = env;
        }

        #region Ordenes de surtimiento
        [RoleFilter(Roles: "ReceptionSignSupplyOrders")]
        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF,UserTypeP")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> OrdenSurtimientoCard(int pageNum = 1, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:OrdenSurtimiento").Value);
                param.Add(new CustomHttpParameter(PageSize, pageSize));
                param.Add(new CustomHttpParameter(PageNumber, pageNum));
                param.Add(new CustomHttpParameter(Search, search));

                var ordenSurtimiento = new DataResult<IEnumerable<SupplyOrderDto>>();
                if (_generals.User.UserType != UserTypeOS)
                {
                    param.Add(new CustomHttpParameter(TokenOS, _generals.User.Token));
                    ordenSurtimiento = await _utility.GetItem<DataResult<IEnumerable<SupplyOrderDto>>>(GetListOSInterno, param);
                }
                else
                {
                    param.Add(new CustomHttpParameter(CreditorNumberOS, _generals.User.CreditorNumber));
                    ordenSurtimiento = await _utility.GetItem<DataResult<IEnumerable<SupplyOrderDto>>>(GetListOSProveedor, param);
                }
                
                if (ordenSurtimiento.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    ordenSurtimiento.Pager = new Pager(ordenSurtimiento?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={ordenSurtimiento.Pager.TotalItems}");
                    ordenSurtimiento.Pager = new Pager(ordenSurtimiento.Pager.TotalItems, pageNum, pageSize);
                }
                ViewBag.PDFCheckDisabled = Convert.ToBoolean(_configuration[PDFCheck]);
                ViewBag.Search = search;
                return PartialView("_OrdenSurtimientoCard", ordenSurtimiento);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorOrdenSurtimientoCard });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Firmar([FromBody]SupplyOrderDto model)
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
                    documentoBEId = model.SupplyOrderID,
                    paquete = new CrearPaquete()
                    {
                        Titulo = $"{TipoDocumentoOS.ToUpper()}_{model.SAPOrder.ToString()}",
                        Descripcion = model.SAPOrder.ToString(),
                        EnProcesoFirma = true,
                        TotalFirmantes = FirmateUnico,

                        Documentos = new List<Documento> {
                            new Documento
                            {
                                Descripcion = $"{TipoDocumentoOS.ToUpper()}_{model.SAPOrder.ToString()}",
                                CodigoTipoDocumento = TipoDocumentoOS.ToUpper(),
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
                return Json(new { success = true, message = FirmaExitosa, documentoResult.Data });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = ErrorFirmado });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CompletarFirma([FromBody]Externos2Dto externos)
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
                var completafirma = await _utility.Post(externosCompletaFirma, PostCompletaFirmaOS);
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

        #endregion
    }
}
