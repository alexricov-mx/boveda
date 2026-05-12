using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.IntegracionEFirma;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.FirmaDocumentos.Repositories
{
    public class ESignRepository : BaseSQLServerSqlRepository, IESignRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IDocumentoFirmadoRepository _documentoFirmadoRepository;
        private readonly RestClient client;
        private readonly HttpClient _client;
        public ESignRepository(string cnnString, IConfiguration configuration, IDocumentoFirmadoRepository documentoFirmadoRepository) : base(cnnString)
        {
            _configuration = configuration;
            _documentoFirmadoRepository = documentoFirmadoRepository;
            client = new RestClient(_configuration["eSign:EndpointNet6:url"]);
            
            HttpClientHandler _Puente = new HttpClientHandler();
            _Puente.ClientCertificateOptions = ClientCertificateOption.Manual;
            _Puente.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            };

            _client = new HttpClient(_Puente);
            _client.BaseAddress = new Uri(_configuration["eSign:EndpointNet6:url"]);
        }
        public async Task<DataResult<UsuarioDto>> GetValidaOCreaUsuarioAsync(UsuarioDto usuario)
        {
            var resultItemIenum = new DataResult<UsuarioDto>()
            {                
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso GetValidaOCreaUsuarioAsync"
            };

            /* TODO: 
             *       1.- Buscar usuario -> correo, RFC, ficha   GET /integracion/usuario
             *           evaluar que si es mi usuario (deben coincidir las 3 cosas) 
             *       2.- Si lo encuentra, regreo el ID
             *       
             *       3.- No lo encontro, Crear usuario POST /integracion/usuario
             * 
             * 
             *      TODO: hacer el modelo para POST
             */


            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"usuario?correos={usuario.Correo}");
                request.Headers.Add("Accept", "application/json");                
                request.Headers.Add("X-BerBackend-ApiKey", _configuration.GetSection("eSign:EndpointNet6:Apikey").Value);
                
                HttpResponseMessage response = await _client.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // No lo encuentra, lo crea

                    CrearFirmante firmante = new CrearFirmante { 
                        Nombre = usuario.Nombre,
                        Correo= usuario.Correo,
                        Extension= string.IsNullOrEmpty(usuario.Extension) ? 0: int.Parse(usuario.Extension),
                        Rfc = usuario.Rfc,
                        Ficha = usuario.Ficha
                    };

                    HttpRequestMessage requestCreate = new HttpRequestMessage(HttpMethod.Post, "usuario");
                    requestCreate.Headers.Add("X-BerBackend-ApiKey", _configuration.GetSection("eSign:EndpointNet6:Apikey").Value);
                    //requestCreate.Headers.Add("content-type", "application/json");
                    requestCreate.Headers.Add("Accept", "application/json");
                    requestCreate.Content = JsonContent.Create(firmante);
                    //requestCreate.Content.Add("paqueteFirmante", JsonConvert.SerializeObject(firmante), ParameterType.RequestBody);
                    response = await _client.SendAsync(requestCreate);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.StatusCode}";
                        resultItemIenum.Status = HttpStatusCode.BadRequest;
                        return resultItemIenum;
                    }
                }
                try
                {
                    // Si existe el usuario (request), lo regresa, si no existe (requestCreate), lo crea y lo regresa
                    string responseString = await response.Content.ReadAsStringAsync();
                    IEnumerable<UsuarioExtendidoDto> responseEfirma = JsonConvert.DeserializeObject<IEnumerable<UsuarioExtendidoDto>>(responseString);
                    UsuarioExtendidoDto resp = responseEfirma.FirstOrDefault();

                    resultItemIenum.Data = new UsuarioDto {
                        Id = resp.IdUsuario,
                        Rfc = resp.Rfc,
                        Ficha = resp.Ficha,
                        Nombre = resp.Nombre,
                        Correo = resp.Correo,
                        Extension = resp.Extension.ToString(),
                        estatusdesc = resp.Estatus == EstatusUsuario.Activo ? "ACTIVO" : "NO-ACTIVO",
                        UsuarioAlta = resp.IdCreador,
                        FechaAlta = resp.FechaCreacion
                    };
                    return resultItemIenum;
                }
                catch (Exception)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.StatusCode}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
            }
            catch (Exception ex)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItemIenum.Status = HttpStatusCode.BadRequest;
                return resultItemIenum;
            }
        }
        public async Task<DataResult<ExternosDto>> PostDocumentoAsync(ExternosDto externos, IFormFile documentoPDF)
        {
            DataResult<ExternosDto> resultItemIenum = new DataResult<ExternosDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso PostDocumentoAsync"
            };
            try
            {
                var client = new RestClient(_configuration["eSign:Endpoint:url"]);
                var request = new RestRequest("Externos/Documento", Method.Post) { RequestFormat = DataFormat.Json, AlwaysMultipartFormData = true };
                request.AddHeader("ApiKey", _configuration.GetSection("eSign:Endpoint:Apikey").Value);
                request.AddHeader("Content-Type", "multipart/form-data");
                request.AddHeader("Accept", "application/json");
                using (var ms = new MemoryStream())
                {
                    documentoPDF.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    request.AddFile("model.DocumentoPDF", fileBytes, documentoPDF.FileName);// Byte Array                    
                }
                request.AddParameter("model.paquete", JsonConvert.SerializeObject(externos.paquete));
                request.AddParameter("model.documentoRepositorio", JsonConvert.SerializeObject(externos.documentoRepositorio));
                request.AddParameter("model.paqueteFirmante", JsonConvert.SerializeObject(externos.paqueteFirmante));
                var response = await client.ExecuteAsync(request);
                try
                {
                    ExternosDto resp = JsonConvert.DeserializeObject<ExternosDto>(response.Content);
                    resultItemIenum.Data = resp;

                    // Al enviar el documento a eSign, creamos el paquete
                    resp.paquete = externos.paquete;
                    resp.usuario = externos.usuario;
                    resp.documentoFirmaBE = externos.documentoFirmaBE;
                    var resultDocumento = await _documentoFirmadoRepository.CreaPaqueteAsync(resp, "1");
                    if (resultDocumento.Equals("EXITO"))
                        resultItemIenum.Message = "Paquete creado con exito";
                    return resultItemIenum;
                }
                catch (Exception)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
            }
            catch (Exception ex)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItemIenum.Status = HttpStatusCode.BadRequest;
                return resultItemIenum;
            }
        }
        // Dto paqueteFirmantes
        public async Task<DataResult<ExternosDto>> PostAgregaFirmanteAsync(ExternosDto externos)
        {
            DataResult<ExternosDto> resultItemIenum = new DataResult<ExternosDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso PostAgregaFirmanteAsync"
            };
            try
            {
                //var client = new RestClient("https://localhost:62117/api/Externos/AgregaFirmante");
                var client = new RestClient(_configuration["eSign:Endpoint:url"]);
                var request = new RestRequest("Externos/AgregaFirmante", Method.Post);
                request.AddHeader("ApiKey", _configuration.GetSection("eSign:Endpoint:Apikey").Value);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Accept", "application/json");
                request.AddParameter("paqueteFirmante", JsonConvert.SerializeObject(externos.paqueteFirmante), ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);

                try
                {
                    var resp = JsonConvert.DeserializeObject<ExternosDto>(response.Content);
                    if (resp != null)
                    {
                        resultItemIenum.Status = HttpStatusCode.OK;
                    }
                    else
                    {
                        resultItemIenum.Status = HttpStatusCode.NoContent;
                    }
                    resp.usuario = externos.usuario;
                    resp.paquete = externos.paquete;
                    resp.documentoFirmaBE = externos.documentoFirmaBE;
                    var resultDocumento = await _documentoFirmadoRepository.CreaPaqueteAsync(resp, "2");
                    if (resultDocumento.Equals("EXITO"))
                        resultItemIenum.Message = "Paquete creado con exito - PostAgregaFirmanteAsync";

                    resultItemIenum.Data = resp;
                    return resultItemIenum;
                }
                catch (Exception)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
            }
            catch (Exception ex)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItemIenum.Status = HttpStatusCode.BadRequest;
                return resultItemIenum;
            }
        }
        // Dto documentoFirma
        public async Task<DataResult<ExternosDto>> PostFirmaAsync(ExternosDto externos, string orden)
        {

            /*
             *  TODO: registrar la firma
             *  Cambiar libreria para llamado rest
             *              
             */

            DataResult<ExternosDto> resultItemIenum = new DataResult<ExternosDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso PostFirmaAsync"
            };

            var correlation = await _documentoFirmadoRepository.GetDocumentoFirmadoAsync(externos.documentoFirmaBE.documentoBEId, int.Parse(orden));
            
            var firmaPaquete = new FirmarPaquete
            {
                IdPaquete = externos.paqueteDocumento.PaqueteId,
                IdCorrelacion = correlation.Data.DocumentoFirmadoID,
                IdUsuarioFirmante = externos.usuario.Id,
                FiguraFirmante = $"FIRMA{orden}",
                CertificadoFirmanteBase64 = externos.documentoFirma.CertificadoB64,
                Firmas = new List<Firma>()
                {
                    new Firma
                    {
                        IdDocumento = externos.paqueteDocumento.DocumentoId,
                        Hash = externos.paqueteDocumento.Hash,
                        IdCorrelacion = correlation.Data.DocumentoFirmadoID
                    }
                }
            };
            var json = JsonConvert.SerializeObject(firmaPaquete);

            try
            {
                // TODO: Cambiar endpoint a net6
                //var client = new RestClient(_configuration["eSign:Endpoint:url"]);
                //var request = new RestRequest("Externos/Firma", Method.Post);
                //request.AddHeader("ApiKey", _configuration.GetSection("eSign:Endpoint:Apikey").Value);
                //request.AddHeader("content-type", "application/json");
                //request.AddHeader("Accept", "application/json");
                //request.AddParameter("documentoFirma", JsonConvert.SerializeObject(externos.documentoFirma), ParameterType.RequestBody);


                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "paquete/firma");
                request.Headers.Add("X-BerBackend-ApiKey", _configuration.GetSection("eSign:EndpointNet6:Apikey").Value);
                request.Headers.Add("Accept", "application/json");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.SendAsync(request);

                try
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador.";
                        resultItemIenum.Status = HttpStatusCode.BadRequest;
                        return resultItemIenum;
                    }

                    string responseString = await response.Content.ReadAsStringAsync();
                    FirmarPaqueteResult resp = JsonConvert.DeserializeObject<FirmarPaqueteResult>(responseString);

                    // TODO: actualizo de exito la firma por correlationid

                    // cuando se hace la firma, actualizamos el paquete
                    string resultDocumento = await _documentoFirmadoRepository.ActualizaFirmaByCorrelationIdAsync(
                                                                                resp.Documentos.FirstOrDefault().IdCorrelacion);
                    if (resultDocumento.Equals("EXITO"))
                        resultItemIenum.Message = "Documento firma registrado con exito";
                    return resultItemIenum;

                }
                catch (Exception)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
            }
            catch (Exception ex)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItemIenum.Status = HttpStatusCode.BadRequest;
                return resultItemIenum;
            }
        }

       


        // NO SE DEBERIA ESTAR USANDO
        // TODO: VERIFICAR Y BORRAR
        public async Task<DataResult<string>> ConsultaEstadoOCSP(IFormFile cerClientePath)
        {
            DataResult<string> resultItem = new DataResult<string>()
            {
                Status = HttpStatusCode.OK,
                Message = "ConsultaEstadoOCSP"
            };

            try
            {
                var client = new RestClient(_configuration["eSign:Endpoint:url"]);
                var request = new RestRequest("Certificado/ConsultaEstadoOCSP", Method.Post) { RequestFormat = DataFormat.Json, AlwaysMultipartFormData = true };
                request.AddHeader("ApiKey", _configuration.GetSection("eSign:Endpoint:Apikey").Value);
                request.AddHeader("Content-Type", "multipart/form-data");
                request.AddHeader("Accept", "application/json");
                using (var ms = new MemoryStream())
                {
                    cerClientePath.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    request.AddFile("cerClientePath", fileBytes, cerClientePath.FileName);// Byte Array
                }
                var response = await client.ExecuteAsync(request);

                try
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        resultItem.Message = "Ocurrió un problema al validar el cerfificado, favor de intentar más tarde.";
                        resultItem.Status = HttpStatusCode.BadRequest;
                        return resultItem;
                    }
                    string resp = JsonConvert.DeserializeObject<string>(response.Content);
                    resultItem.Data = resp;
                    return resultItem;
                }
                catch (Exception)
                {
                    resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                    resultItem.Status = HttpStatusCode.BadRequest;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<DataResult<string>> ValidaCertificado(string CertificadoB64)
        {
            DataResult<string> resultItem = new DataResult<string>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso ConsultaPaqueteDocumentoId"
            };


            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "certificado/validacion");
            request.Headers.Add("X-BerBackend-ApiKey", _configuration.GetSection("eSign:EndpointNet6:Apikey").Value);            
            
            StringContent content = new StringContent(CertificadoB64,null,"text/plain");
            request.Content = content;            

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.StatusCode}";
                resultItem.Status = HttpStatusCode.BadRequest;
            }
            else
            {
                string res = await response.Content.ReadAsStringAsync();
                resultItem.Data = "Certificado Valido";
            }

            return resultItem;
        }

        public async Task<DataResult<ExternosDto>> ConsultaPaqueteDocumentoId(int PaqueteId)
        {
            DataResult<ExternosDto> resultItemIenum = new DataResult<ExternosDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso ConsultaPaqueteDocumentoId",
                Data = new ExternosDto()
            };

            try
            {
                //var client = new RestClient("https://localhost:62117/api/Externos/ValidaOCreaUsuario");
                //var client = new RestClient(_configuration["eSign:Endpoint:url"]);
                var request = new RestRequest($"PaquetesDocumentos/{PaqueteId}", Method.Get);
                request.AddHeader("ApiKey", _configuration.GetSection("eSign:Endpoint:Apikey").Value);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Accept", "application/json");
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.ErrorMessage}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
                try
                {
                    IEnumerable<PaqueteDocumentoDetalleDto> paqueteDocumento = JsonConvert.DeserializeObject<IEnumerable<PaqueteDocumentoDetalleDto>>(response.Content);
                    resultItemIenum.Data.paqueteDocumento = paqueteDocumento.FirstOrDefault();
                    return resultItemIenum;
                }
                catch (Exception)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.ErrorMessage}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
            }
            catch (Exception ex)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItemIenum.Status = HttpStatusCode.BadRequest;
                return resultItemIenum;
            }
        }

        public async Task<DataResult<ArchivoPDFDto>> RecuperaPDFFirmado(int PaqueteId, int DocumentoId)
        {
            DataResult<ArchivoPDFDto> resultItemIenum = new DataResult<ArchivoPDFDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso ConsultaPaqueteDocumentoId"
            };

            try
            {
                //var client = new RestClient("https://firmades-back.pemex.com/api/");
                //var client = new RestClient(_configuration["eSign:EndpointNet6:url"]);
                //var request = new RestRequest($"Representacion/GetRepresentacionPd2fAsync/{PaqueteId}/{DocumentoId}", Method.GET);
                //var request = new RestRequest($"representacion-grafica?idDocumento={DocumentoId}", Method.Get);
                var request = new RestRequest($"documento/{DocumentoId}/firma/representacion-grafica", Method.Get);
                request.AddHeader("X-BerBackend-ApiKey", _configuration.GetSection("eSign:EndpointNet6:Apikey").Value);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Accept", "application/json");
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.ErrorMessage}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
                try
                {
                    ArchivoPDFDto archivoPDF = new ArchivoPDFDto { ARCHIVO = Convert.ToBase64String(response.RawBytes) };
                    resultItemIenum.Data = archivoPDF;
                    return resultItemIenum;
                }
                catch (Exception)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.ErrorMessage}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
            }
            catch (Exception ex)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItemIenum.Status = HttpStatusCode.BadRequest;
                return resultItemIenum;
            }
        }

        public async Task<DataResult<ArchivoPDFDto>> RecuperaPDFAPFirmado(int PaqueteId, int DocumentoId)
        {
            DataResult<ArchivoPDFDto> resultItemIenum = new DataResult<ArchivoPDFDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso ConsultaPaqueteDocumentoId"
            };

            try
            {
                //var client = new RestClient("https://localhost:62117/api/Externos/ValidaOCreaUsuario");
                var client = new RestClient(_configuration["eSign:Endpoint:url"]);
                var request = new RestRequest($"Representacion/GetRepresentacionPdf2Async/{PaqueteId}/{DocumentoId}", Method.Get);
                request.AddHeader("ApiKey", _configuration.GetSection("eSign:Endpoint:Apikey").Value);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Accept", "application/json");
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.ErrorMessage}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
                try
                {
                    ArchivoPDFDto archivoPDF = new ArchivoPDFDto { ARCHIVO = Convert.ToBase64String(response.RawBytes) };
                    resultItemIenum.Data = archivoPDF;
                    return resultItemIenum;
                }
                catch (Exception)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.ErrorMessage}";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }
            }
            catch (Exception ex)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItemIenum.Status = HttpStatusCode.BadRequest;
                return resultItemIenum;
            }
        }

        public async Task<DataResult<ExternosDto>> ConsultaDocumentoFirmaAsync(int PaqueteId, int DocumentoId, int UsuarioId)
        {
            // TODO: Consultar paquete apuntando a la nueva API NET 6
            DataResult<ExternosDto> resultItem = new DataResult<ExternosDto>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso ConsultaDocumentoFirmaAsync"
            };

            try
            {
                //var client = new RestClient(_configuration["eSign:Endpoint:url"]);
                //var client = new RestClient(_configuration["eSign:EndpointNet6:url"]);
                var request = new RestRequest($"Externos/DocumentoFirma", Method.Get);
                //request.AddHeader("ApiKey", _configuration.GetSection("eSign:Endpoint:Apikey").Value);
                request.AddHeader("X-BerBackend-ApiKey", _configuration.GetSection("eSign:EndpointNet6:Apikey").Value);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Accept", "application/json");
                request.AddParameter("paqueteid", PaqueteId);
                request.AddParameter("documentoid", DocumentoId);
                request.AddParameter("usuarioid", UsuarioId);
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.ErrorMessage}";
                    resultItem.Status = HttpStatusCode.BadRequest;
                    return resultItem;
                }
                try
                {
                    resultItem.Data = JsonConvert.DeserializeObject<ExternosDto>(response.Content);
                    var Cert = JsonConvert.DeserializeObject<CertificadoDto>(resultItem.Data.documentoFirma.InfoFirmado);
                    resultItem.Data.documentoFirma.Certificado = Cert.Certificado;
                    return resultItem;
                }
                catch (Exception)
                {
                    resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.ErrorMessage}";
                    resultItem.Status = HttpStatusCode.BadRequest;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                resultItem.Status = HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        /// <summary>
        /// Integracion API NET 6
        /// Se recibiria el id de correlacion
        /// application/pdf
        /// </summary>
        /// <param name="externos"></param>
        /// <param name="documentoPDF"></param>
        /// <returns></returns>
        //public async Task<DataResult<ExternosDto>> PostDocumentoByCorrelationIDAsync(Guid CorrelationID, ExternosDto externos, IFormFile documentoPDF)
        public async Task<DataResult<CrearPaqueteResult>> PostDocumentoEFirmaAsync(            
                                                                            string NomArchivo,
                                                                            CrearPaquete paquete, 
                                                                            IFormFile documentoPDF)
        {
            DataResult<CrearPaqueteResult> resultItemIenum = new DataResult<CrearPaqueteResult>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso PostDocumentoAsync"
            };

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "paquete");
            request.Headers.Add("X-BerBackend-ApiKey", _configuration.GetSection("eSign:EndpointNet6:Apikey").Value);
            request.Headers.Add("Accept", "application/json");

            string json = JsonConvert.SerializeObject(paquete);
            using MultipartFormDataContent content = new MultipartFormDataContent();
            using MemoryStream ms = new MemoryStream();
            documentoPDF.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            content.Add(new StreamContent(ms), $"documento_{paquete.IdCorrelacion}", NomArchivo); 
            content.Add(new StringContent(json), "paquete");
            request.Content = content;
            _client.BaseAddress = new Uri(_configuration["eSign:EndpointNet6:url"]);
            HttpResponseMessage response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                resultItemIenum.Data = JsonConvert.DeserializeObject<CrearPaqueteResult>(responseString);
                resultItemIenum.Message = "Paquete creado con exito";
            }
            else
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador.";
                resultItemIenum.Status = HttpStatusCode.BadRequest;
            }
            return resultItemIenum;                              

        }

        public async Task<DataResult<string>> PostAgregaFirmanteEFirmaAsync(AgregaFirmante agregaFirmante)
        {
            DataResult<string> resultItem = new DataResult<string>()
            {                
                Message = "Se agregó el usuario de manera exitoa"                
            };

            string json = JsonConvert.SerializeObject(agregaFirmante);
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "firmante-paquete");
            request.Headers.Add("X-BerBackend-ApiKey", _configuration.GetSection("eSign:EndpointNet6:Apikey").Value);
            request.Headers.Add("Accept", "application/json");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.SendAsync(request);

            try
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador.";
                    resultItem.Status = HttpStatusCode.BadRequest;
                    return resultItem;
                }

                resultItem.Status = response.StatusCode;
                resultItem.Data = "Exito";

                return resultItem;

            }
            catch (Exception)
            {
                resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                resultItem.Status = HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        /// <summary>
        /// EFirma v2
        /// Registro de Firma
        /// </summary>
        /// <param name="firmaPaquete"></param>
        /// <returns></returns>
        public async Task<DataResult<FirmarPaqueteResult>> PostFirmaEFirmaAsync(FirmarPaquete firmaPaquete)
        {
            DataResult<FirmarPaqueteResult> resultItemIenum = new DataResult<FirmarPaqueteResult>()
            {
                Status = HttpStatusCode.OK,
                Message = "Envío Exitoso PostFirmaAsync"
            };

            string json = JsonConvert.SerializeObject(firmaPaquete);
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "paquete/firma");
            request.Headers.Add("X-BerBackend-ApiKey", _configuration.GetSection("eSign:EndpointNet6:Apikey").Value);
            request.Headers.Add("Accept", "application/json");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.SendAsync(request);

            try
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador.";
                    resultItemIenum.Status = HttpStatusCode.BadRequest;
                    return resultItemIenum;
                }

                string responseString = await response.Content.ReadAsStringAsync();
                FirmarPaqueteResult resp = JsonConvert.DeserializeObject<FirmarPaqueteResult>(responseString);

                // TODO: actualizo de exito la firma por correlationid

                // cuando se hace la firma, actualizamos el paquete
                string resultDocumento = await _documentoFirmadoRepository.ActualizaFirmaByCorrelationIdAsync(
                                                                            resp.Documentos.FirstOrDefault().IdCorrelacion);
                if (resultDocumento.Equals("EXITO"))
                    resultItemIenum.Message = "Documento firma registrado con exito";
                return resultItemIenum;

            }
            catch (Exception)
            {
                resultItemIenum.Message = $"Ocurrio un problema. Contacta a tu administrador. {response.Content}";
                resultItemIenum.Status = HttpStatusCode.BadRequest;
                return resultItemIenum;
            }
        }

        public async Task<DataResult<CrearPaqueteResult>> GetDocumentoEFirmaAsync(Guid IdCorrelacion)
        {
            //paquete/{ID}/creado
            DataResult<CrearPaqueteResult> resultItem = new DataResult<CrearPaqueteResult>()
            {
                Message = "Se agregó el usuario de manera exitoa"
            };

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"paquete/{IdCorrelacion}/creado");
            request.Headers.Add("X-BerBackend-ApiKey", _configuration.GetSection("eSign:EndpointNet6:Apikey").Value);
            request.Headers.Add("Accept", "application/json");
            
            HttpResponseMessage response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                resultItem.Data = JsonConvert.DeserializeObject<CrearPaqueteResult>(responseString);
                resultItem.Message = "Paquete creado con exito";
            }

            return resultItem;
        }

        public async Task<DataResult<ArchivoPDFDto>> RecuperaPDFFirmadoEFirmaAsync(Guid IdCorrelacion)
        {
            DataResult<ArchivoPDFDto> resultItem = new DataResult<ArchivoPDFDto>()
            {
                Message = "Se obtiene documento de EFirma de manera exitosa"
            };            
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"documento/{IdCorrelacion}/firma/representacion-grafica");
            request.Headers.Add("X-BerBackend-ApiKey", _configuration.GetSection("eSign:EndpointNet6:Apikey").Value);
            request.Headers.Add("Accept", "application/json");

            HttpResponseMessage response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();                
                ArchivoPDFDto archivoPDF = new ArchivoPDFDto { ARCHIVO = Convert.ToBase64String(responseBytes) };
                resultItem.Data = archivoPDF;
                resultItem.Status = response.StatusCode;
                return resultItem;
            }
            else
            {
                resultItem.Message = "No se puede recuperar el documento PDF de EFirma";
                resultItem.Status = HttpStatusCode.NotFound;
            }
            return resultItem;
        }
    }
}
