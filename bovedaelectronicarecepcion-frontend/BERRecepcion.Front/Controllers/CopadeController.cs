using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Models.IntegracionEFirma;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BERRecepcion.Front.Models.CrearPaquete;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    //[AllowAnonymous]
    public class CopadeController : Controller
    {
        //Variables de trabajo
        #region Firmantes
        private static readonly string ExploracionProduccion = "PEP";
        private static readonly int FirmateUnico = 1;
        private static readonly string Corporativo = "PCORP";
        private static readonly int DosFirmates = 2;
        #endregion

        #region Servicios
        private static readonly string GetListFiltroCopade = "Copade/GetListaFiltroCopadesAsync";
        private static readonly string GetDocumentoFirmadoCO = "DocumentoFirmado/GetDocumentoFirmadoAsync";
        private static readonly string GetRecuperaPDFFirmado = "ESign/RecuperaPDFFirmadoEFirmaAsync";
        private static readonly string PostValidaOCreaUsuarios = "ESign/ValidaOCreaUsuarios";        
        private static readonly string PostFirmaUnoCO = "Copade/FirmaUnoAsync";
        private static readonly string PostFirmaDosCO = "Copade/FirmaDosAsync";
        private static readonly string PostCompletaFirmaCO = "Copade/CompletaFirmaAsync";
        #endregion

        #region Paginado
        private static readonly string PageSize = "pageSize";
        private static readonly string PageNumber = "pageNum";
        private static readonly string Search = "search";
        private static readonly string UserId = "UserID";
        #endregion

        #region Mensajes
        private static readonly string FirmaExitosa = "Firma exitosa";
        private static readonly string ErrorCargaCopade = "Ocurrió un error al cargar los copades, por favor intente más tarde";
        private static readonly string ErrorFirmado = "Ocurrió un error al firmar, por favor intente más tarde";
        private static readonly string ErrorEnvioCorreo = "La firma se completó satisfactoriamente, pero ocurrió un error al realizar el envío del correo electrónico.";
        private static readonly string FirmaCompleta = "La firma se completó de manera satisfactoria.";
        #endregion

        #region Identificadores
        private static readonly string DoctoBE = "DocumentoBEId";
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        protected readonly IGenerals _generals;
        private readonly IHostEnvironment _env;
        private static readonly string TipoDocumento = "copade";        
        #endregion

        //Variables de trabajo

        public CopadeController(IConfiguration configuration, IRestUtility utility, IGenerals generals, IHostEnvironment env)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
            _env = env;
        }
        [RoleFilter(Roles: "ReceptionSignCopade")]
        [UserTypeFilter("UserTypeS,UserTypeA,UserTypeF")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCopades(int pageNum = 1, string search = null)
        {
            var param = new List<CustomHttpParameter>();

            int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Copade").Value);
            param.Add(new CustomHttpParameter(PageSize, pageSize));
            param.Add(new CustomHttpParameter(PageNumber, pageNum));
            param.Add(new CustomHttpParameter(Search, search));
            param.Add(new CustomHttpParameter(UserId, _generals.User.UserID));
            
            Log.Information("========== INICIO GetCopades ==========");
            Log.Information($"UserID: {_generals.User.UserID}");
            Log.Information($"pageNum: {pageNum}");
            Log.Information($"pageSize: {pageSize}");
            Log.Information($"search: {search ?? "(null)"}");
            Log.Information($"Endpoint: {GetListFiltroCopade}");
            Log.Information($"ApiUrl configurado: {_configuration["ApiUrl"]}");
            
            try
            {
                var CopadeSeguimiento = await _utility.GetItem<DataResult<IEnumerable<CopadeDto>>>(GetListFiltroCopade, param);
                
                Log.Information($"✓ Backend respondió exitosamente. Registros: {CopadeSeguimiento?.Data?.Count() ?? 0}");
                
                // Verificar que Pager no sea null
                if (CopadeSeguimiento.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    CopadeSeguimiento.Pager = new Pager(CopadeSeguimiento?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={CopadeSeguimiento.Pager.TotalItems}");
                    // Si llegamos aquí, RestUtility recibió HTTP 200 Y tenemos Pager
                    CopadeSeguimiento.Pager = new Pager(CopadeSeguimiento.Pager.TotalItems, pageNum, pageSize);
                }
                
                ViewBag.Search = search;
                return PartialView("_CopadeTable", CopadeSeguimiento);
            }
            catch (ApplicationException ex)
            {
                // RestUtility lanza excepción cuando StatusCode != 200
                Log.Error("========== ERROR ApplicationException ==========");
                Log.Error($"Mensaje: {ex.Message}");
                Log.Error($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Log.Error($"InnerException: {ex.InnerException.Message}");
                    Log.Error($"InnerException StackTrace: {ex.InnerException.StackTrace}");
                }
                
                // Extraer código HTTP del mensaje de excepción
                if (ex.Message.Contains("404") || ex.Message.Contains("NotFound"))
                {
                    Log.Warning("Interpretado como 404 - No encontrado");
                    return StatusCode(404, new { message = "No se encontraron copades con los criterios especificados" });
                }
                else if (ex.Message.Contains("400") || ex.Message.Contains("BadRequest"))
                {
                    Log.Warning("Interpretado como 400 - Bad Request");
                    return StatusCode(400, new { message = "Los parámetros de búsqueda son inválidos" });
                }
                else if (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
                {
                    Log.Warning("Interpretado como 401 - Unauthorized");
                    return StatusCode(401, new { message = "No está autorizado para acceder a este recurso" });
                }
                else if (ex.Message.Contains("500") || ex.Message.Contains("InternalServerError"))
                {
                    Log.Error("Interpretado como 500 - Internal Server Error");
                    return StatusCode(500, new { message = ErrorCargaCopade });
                }
                
                // Error genérico de RestUtility - incluir detalles en el log
                Log.Error($"Error NO CATEGORIZADO - Retornando 500");
                Log.Error($"Para depuración, mensaje completo: {ex.ToString()}");
                return StatusCode(500, new { message = ErrorCargaCopade });
            }
            catch (Exception ex)
            {
                Log.Error("========== ERROR INESPERADO ==========");
                Log.Error($"Tipo: {ex.GetType().FullName}");
                Log.Error($"Mensaje: {ex.Message}");
                Log.Error($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Log.Error($"InnerException Tipo: {ex.InnerException.GetType().FullName}");
                    Log.Error($"InnerException Mensaje: {ex.InnerException.Message}");
                    Log.Error($"InnerException StackTrace: {ex.InnerException.StackTrace}");
                }
                
                Log.Error($"ToString completo: {ex.ToString()}");
                return StatusCode(500, new { message = ErrorCargaCopade });
            }
        }

        [HttpPost]
        public async Task<IActionResult> pdfPreview(PICopadeRequestDto dto)
        {
            Log.Information("========== INICIO pdfPreview ==========");
            Log.Information($"CopadeId: {dto.CopadeId}");
            Log.Information($"Functionary1SignDate: {dto.Functionary1SignDate}");
            
            DtoPdfPreviewData dtoPreview = new DtoPdfPreviewData();
            dtoPreview.data = dto; 

            var dataResult = new DataResult<DtoPdfPreviewData>
            {
                Data = dtoPreview,
                User = _generals.User
            };
            dtoPreview.UsuarioModificador = _generals.User.Token;
            
            try
            {
                if (dto.Functionary1SignDate)  // Para identificar sí es primer firma o segunda firma
                {
                    Log.Information("Recuperando PDF de segunda firma");
                    // aqui recuperamos el PDF  - segunda firma
                    // ESign/RecuperaPDFFirmadoAsync
                    // recuperar el paqueteId y documentoId
                    var param = new List<CustomHttpParameter>();                    
                    param.Add(new CustomHttpParameter(DoctoBE, dto.CopadeId));
                    
                    var documentoFirmado = await _utility.GetItem<DataResult<DocumentoFirmadoDto>>(GetDocumentoFirmadoCO, param);
                    
                    if (documentoFirmado.Status != System.Net.HttpStatusCode.OK)
                    {
                        Log.Error($"Error al obtener documento firmado: {documentoFirmado.Status}");
                        return StatusCode((int)documentoFirmado.Status, new { message = documentoFirmado.Message ?? "Error al recuperar documento firmado" });
                    }
                    
                    var resultRecuperado = await _utility.GetItem<DataResult<ArchivoPDFDto>>($"{GetRecuperaPDFFirmado}/{documentoFirmado.Data.DocumentoFirmadoID}");

                    if (resultRecuperado.Status == System.Net.HttpStatusCode.OK)
                    {
                        Log.Information("✓ PDF recuperado exitosamente");
                        return Content(resultRecuperado.Data.ARCHIVO, "text/plain");
                    }
                    else
                    {
                        Log.Error($"Error al recuperar PDF: {resultRecuperado.Status}");
                        return StatusCode((int)resultRecuperado.Status, new { message = resultRecuperado.Message ?? "Error al recuperar PDF" });
                    }
                }
                else
                {
                    Log.Information("Generando PDF de primera firma");
                    // armamos nuestro PDF - primer firma
                    var result = await _utility.Post<DataResult<DtoPdfPreviewData>>(dataResult, TipoDocumento);
                    
                    Log.Information($"POST completado - HasData: {result != null}, Data: {result?.Data != null}");
                    
                    // Si Post tuvo éxito, verificamos que tenga datos
                    if (result?.Data?.result != null && result.Data.result.Any())
                    {
                        var pdfBase64 = result.Data.result.FirstOrDefault();
                        Log.Information($"✓ PDF generado exitosamente - Length: {pdfBase64?.Length ?? 0}");
                        return Content(pdfBase64, "text/plain");
                    }
                    else
                    {
                        Log.Error("Error: El resultado no contiene datos de PDF");
                        return StatusCode(500, new { message = result?.Message ?? "Error al generar PDF" });
                    }
                }                
            }
            catch (ApplicationException ex)
            {
                // RestUtility lanza excepción cuando StatusCode != 200
                Log.Error("========== ERROR ApplicationException ==========");
                Log.Error($"Mensaje: {ex.Message}");
                Log.Error($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error al procesar la solicitud de PDF" });
            }
            catch(Exception ex)
            {
                Log.Error("========== ERROR INESPERADO ==========");
                Log.Error($"Tipo: {ex.GetType().FullName}");
                Log.Error($"Mensaje: {ex.Message}");
                Log.Error($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error al generar vista previa del PDF" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Firmar([FromBody] CopadeDto model)
        {
            try
            {
                //Validar usuario
                DataResult<UsuarioDto> responseExternos = new DataResult<UsuarioDto>
                {
                    Data = _generals.Usuario
                };
                DataResult<UsuarioDto> responseUsuario = await _utility.Post(responseExternos, PostValidaOCreaUsuarios);

                if (responseUsuario.Status != System.Net.HttpStatusCode.OK || responseUsuario.Data == null)
                {
                    var status = responseUsuario.Status;
                    return StatusCode((int)status, new { message = responseUsuario.Message });
                }

                ////Peticiones para firma
                DataResult<Externos2Dto> dataResult = new DataResult<Externos2Dto>() { User = _generals.User };
                DataResult<Externos2Dto> documentoResult = new DataResult<Externos2Dto>();
                dataResult.Data = new Externos2Dto
                {
                    copade = model,
                    usuario = responseUsuario.Data,
                    usuarioBEId = _generals.User.UserID,   
                    documentoBEId = model.CopadeID,
                    paquete = new CrearPaquete()
                    {
                        Titulo = $"{TipoDocumento.ToUpper()}_{model.clave.ToString()}_{model.Exercise.ToString()}_{model.SapOrder.ToString()}_{model.Reception.ToString()}",
                        Descripcion = model.Reception.ToString(),
                        EnProcesoFirma = true,
                        TotalFirmantes = model.clave.ToUpper().Equals(ExploracionProduccion) ||
                                         model.clave.ToUpper().Equals(Corporativo) ? FirmateUnico : DosFirmates,

                        Documentos = new List<Documento> {
                            new Documento
                            {
                                Descripcion = $"{TipoDocumento.ToUpper()}_{model.clave.ToString()}_{model.Exercise.ToString()}_{model.SapOrder.ToString()}_{model.Reception.ToString()}",
                                CodigoTipoDocumento = TipoDocumento.ToUpper(),
                            }
                        },
                        Firmantes = new List<Firmante>
                        {
                            new Firmante
                            {
                                IdUsuario = responseUsuario.Data.Id,
                                Figura = string.IsNullOrWhiteSpace(model.Functionary1SignDate.ToString()) ?
                                                                    FirmateUnico.ToString() : DosFirmates.ToString()
                            }
                        }
                    }
                };                

                if (string.IsNullOrWhiteSpace(model.Functionary1SignDate.ToString()))
                {
                    documentoResult = await _utility.Post(dataResult, PostFirmaUnoCO);
                    
                    // Check if the request was successful
                    if (documentoResult.Status != System.Net.HttpStatusCode.OK)
                    {
                        var status = documentoResult.Status;
                        return StatusCode((int)status, new { message = documentoResult.Message });
                    }
                    
                    documentoResult.Data.usuario = responseUsuario.Data;
                }
                else
                {
                    if (model.clave == ExploracionProduccion || model.clave == Corporativo)
                    {
                        return Json(new { success = true, message = FirmaExitosa, documentoResult.Data });
                    }

                    List<CustomHttpParameter> param = new List<CustomHttpParameter>();
                    param.Add(new CustomHttpParameter(DoctoBE, model.CopadeID));
                    DataResult<DocumentoFirmadoDto> documentoFirmado = await _utility.GetItem<DataResult<DocumentoFirmadoDto>>(GetDocumentoFirmadoCO, param);
                    if (documentoFirmado.Status != System.Net.HttpStatusCode.OK)
                    {
                        var status = documentoFirmado.Status;
                        return StatusCode((int)status, new { message = documentoFirmado.Message });
                    }

                    dataResult.Data.agregarFirmante = new AgregaFirmante();
                    dataResult.Data.agregarFirmante.Firmantes = new List<Firmante>() { 
                        new Firmante ()
                        {
                            IdUsuario = responseUsuario.Data.Id,
                            Figura = DosFirmates.ToString()
                        }
                    };

                    documentoResult = await _utility.Post(dataResult, PostFirmaDosCO);
                    
                    // Check if the second signature request was successful
                    if (documentoResult.Status != System.Net.HttpStatusCode.OK)
                    {
                        var status = documentoResult.Status;
                        return StatusCode((int)status, new { message = documentoResult.Message });
                    }
                }
                return Json(new { success = true, message = FirmaExitosa, documentoResult.Data });

            }
            catch (ApplicationException ex)
            {
                // Handle RestUtility exceptions that may contain HTTP error info
                Log.Error(ex.Message);
                return StatusCode(500, new { message = ErrorFirmado });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, new { message = ErrorFirmado });
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
                DataResult<Externos2Dto> Completafirma = await _utility.Post(externosCompletaFirma, PostCompletaFirmaCO);
                
                // Return proper HTTP status codes based on backend response
                if (Completafirma.Status == System.Net.HttpStatusCode.BadRequest)
                    return StatusCode(400, new { message = Completafirma.Message });
                    
                if (Completafirma.Status == System.Net.HttpStatusCode.Conflict)
                    return StatusCode(409, new { info = true, message = ErrorEnvioCorreo });
                    
                if (Completafirma.Status != System.Net.HttpStatusCode.OK)
                {
                    var status = Completafirma.Status;
                    return StatusCode((int)status, new { message = Completafirma.Message });
                }
                
                return Json(new { success = true, message = FirmaCompleta });

            }
            catch (ApplicationException ex)
            {
                // Handle RestUtility exceptions that may contain HTTP error info
                Log.Error(ex.Message);
                return StatusCode(500, new { message = ErrorFirmado });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, new { message = ErrorFirmado });
            }
        }
       
    }
}
