using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft;
using BERRecepcion.Front.Interfaces;
using Newtonsoft.Json;
using BERRecepcion.Front.Models;
using System.Net;
using Serilog;
using System.Net.Http;
using System.Net.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;

namespace BERRecepcion.Front.Utilities
{
    public class RestUtility : IRestUtility
    {
        private readonly IConfiguration _configuration;
        private readonly RestClient _client;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RestUtility(IConfiguration configuration, ITokenAcquisition tokenAcquisition, IHttpContextAccessor httpContextAccessor = null)
        {
            _configuration = configuration;
            _tokenAcquisition = tokenAcquisition;
            _httpContextAccessor = httpContextAccessor;
            // bloque de codigo temporal para ignorar el certificado vencido
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, SslPolicyErrors) => true
            };
            var httpClient = new HttpClient(handler);
            var options = new RestClientOptions(_configuration["ApiUrl"])
            {
                ConfigureMessageHandler = _ => handler
            };
            _client = new RestClient(options);
        }
  
        /// <summary>
        /// Obtiene el JWT access token via MSAL (ITokenAcquisition).
        /// Maneja renovación silenciosa automática cuando el token expira.
        /// </summary>
        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var scopes = new[] { _configuration["AzureAd:Scopes"] };
                Log.Information($"Solicitando access token via MSAL para scope: {scopes[0]}");
                var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                Log.Information($"✓ Access token obtenido via MSAL (longitud: {accessToken?.Length ?? 0})");
                return accessToken;
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                Log.Warning($"Re-autenticacion requerida (refresh token expirado): {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Log.Error($"Error obteniendo access token via MSAL: {ex.Message}");
                Log.Error($"StackTrace: {ex.StackTrace}");
                return null;
            }
        }
  
        /// <summary>
        /// Get: Obtener elemento
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiEndPoint"></param>
        /// <returns></returns>
        public async Task<T> GetItem<T>(string apiEndPoint, IEnumerable<CustomHttpParameter> parameters = null)
        {
            try
            {
                Log.Information("========== RestUtility.GetItem INICIO ==========");
                Log.Information($"Endpoint: {apiEndPoint}");
                Log.Information($"Base URL: {_client.Options.BaseUrl}");
                
                var request = new RestRequest(apiEndPoint, Method.Get);
                request.AddHeader("ApiKey", _configuration.GetSection("Seguridad:ApiKey").Value);
                
                Log.Information($"ApiKey: {_configuration.GetSection("Seguridad:ApiKey").Value?.Substring(0, 4)}****");
                
                // Agregar JWT token de Azure AD
                var accessToken = await GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                    Log.Information($"Token de autorización agregado (longitud: {accessToken.Length})");
                }
                else
                {
                    Log.Warning("NO se pudo obtener token de Azure AD");
                }
                
                if(parameters != null)
                {
                    Log.Information($"Parámetros: {parameters.Count()}");
                    foreach (var item in parameters)
                    {
                        request.AddParameter(item.Name, item.Value);
                        Log.Information($"  - {item.Name} = {item.Value}");
                    }
                }
                
                Log.Information($"URL completa: {_client.Options.BaseUrl}{apiEndPoint}");
                Log.Information("Ejecutando request...");
                
                var response = await _client.ExecuteAsync(request);
                
                Log.Information($"Respuesta recibida - StatusCode: {response.StatusCode} ({(int)response.StatusCode})");
                Log.Information($"ResponseStatus: {response.ResponseStatus}");
                Log.Information($"IsSuccessful: {response.IsSuccessful}");
                
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    Log.Error($"ErrorMessage: {response.ErrorMessage}");
                }
                
                if (!string.IsNullOrEmpty(response.Content))
                {
                    Log.Information($"Content Length: {response.Content.Length} bytes");
                    Log.Information($"Content (primeros 500 chars): {response.Content.Substring(0, Math.Min(500, response.Content.Length))}");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedItem = JsonConvert.DeserializeObject<T>(response.Content);
                    Log.Information("✓ Deserialización exitosa");
                    return returnedItem;
                }
                
                // Error - no es 200
                string errorInfo = $"Error: Acción:GET { response.ResponseUri } | Info: { response.StatusCode }, {response.StatusDescription} | { response.Content }";
                Log.Error($"ERROR: StatusCode != 200");
                Log.Error($"ErrorInfo completo: {errorInfo}");
                
                var error = new ErrorModel { StatusCode = response.StatusCode, Message = response.ErrorMessage};
                throw new ApplicationException(errorInfo);

            }
            catch (ApplicationException)
            {
                Log.Error("Relanzando ApplicationException");
                throw;
            }
            catch (Exception ex)
            {
                Log.Error($"EXCEPCIÓN EN RestUtility.GetItem: {ex.GetType().FullName}");
                Log.Error($"Mensaje: {ex.Message}");
                Log.Error($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }
        /// <summary>
        /// Get; Obtener IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiEndPoint"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetList<T>(string apiEndPoint, IEnumerable<CustomHttpParameter> parameters = null)
        {
            try
            {
                var request = new RestRequest(apiEndPoint, Method.Get);
                request.AddHeader("ApiKey", _configuration.GetSection("Seguridad:ApiKey").Value);
                
                // Agregar JWT token de Azure AD
                var accessToken = await GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                }
                
                if (parameters != null)
                    foreach (var item in parameters)
                    {
                        request.AddParameter(item.Name, item.Value);
                    }
                var response = await _client.ExecuteAsync(request);
                
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedItem = JsonConvert.DeserializeObject<IEnumerable<T>>(response.Content);
                    return returnedItem;
                }
                string errorInfo = $"Error: Acción:GET { response.ResponseUri } | Info: { response.StatusCode }, {response.StatusDescription} | { response.Content }";

                throw new ApplicationException(errorInfo);
            }
            catch (Exception )
            {
                throw ;
            }
        }
        /// <summary>
        /// Post: Insertar nuevo registro de tipo T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiEndPoint"></param>
        /// <returns></returns>
        public async Task<T> Post<T>(T dto, string apiEndPoint)
        {
            try
            {
                var request = new RestRequest(apiEndPoint, Method.Post);
                //request.AddJsonBody(dto);
                request.AddParameter("application/json", dto, ParameterType.RequestBody);

                request.AddHeader("ApiKey", _configuration.GetSection("Seguridad:ApiKey").Value);
                
                // Agregar JWT token de Azure AD
                var accessToken = await GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                }
                var response = await _client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedItem = JsonConvert.DeserializeObject<T>(response.Content);
                    return returnedItem;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.RequestTimeout || response.StatusCode==0)
                {
                    throw new TimeoutException();
                }
                else
                {
                    string errorInfo = $"Error: Acción:POST { response.ResponseUri } | Info: { response.StatusCode }, {response.StatusDescription} | { response.Content }";
                    throw new ApplicationException(errorInfo);
                }
            }
            catch (Exception )
            {
                throw ;
            }
        }
        /// <summary>
        /// Put: Actualizr registro T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiEndPoint"></param>
        /// <returns></returns>
        public async Task<T> Update<T>(T dto, Guid Id,  string apiEndPoint)
        {
            try
            {
                var request = new RestRequest(string.Concat(apiEndPoint, "/", Id), Method.Put);
                //request.AddJsonBody(dto);
                request.AddParameter("application/json", dto, ParameterType.RequestBody);
                request.AddHeader("ApiKey", _configuration.GetSection("Seguridad:ApiKey").Value);
                
                // Agregar JWT token de Azure AD
                var accessToken = await GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                }
                var response = await _client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedItem = JsonConvert.DeserializeObject<T>(response.Content);
                    return returnedItem;
                }
                string errorInfo = $"Error: Acción:UPDATE { response.ResponseUri } | Info: { response.StatusCode }, {response.StatusDescription} | { response.Content }";

                throw new ApplicationException(errorInfo);
            }
            catch (Exception )
            {
                throw ;
            }
        }
        /// <summary>
        /// Put: Actualizr registro T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiEndPoint"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> UpdateReturnList<T>(T dto, Guid Id, string apiEndPoint)
        {
            try
            {
                var request = new RestRequest(string.Concat(apiEndPoint, "/", Id), Method.Put);
                //request.AddJsonBody(dto);
                request.AddParameter("application/json", dto, ParameterType.RequestBody);
                request.AddHeader("ApiKey", _configuration.GetSection("Seguridad:ApiKey").Value);
                
                // Agregar JWT token de Azure AD
                var accessToken = await GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                }
                var response = await _client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedItem = JsonConvert.DeserializeObject<DataResult<IEnumerable<T>>>(response.Content);
                    return returnedItem.Data;
                }
                string errorInfo = $"Error: Acción:POST { response.ResponseUri } | Info: { response.StatusCode }, {response.StatusDescription} | { response.Content }";

                throw new ApplicationException(errorInfo);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Put: Borrar registro
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiEndPoint"></param>
        /// <returns></returns>
        public async Task<T> Delete<T>(Guid Id, string apiEndPoint)
        {
            try
            {
                var request = new RestRequest(string.Concat(apiEndPoint, "/", Id), Method.Delete);
                request.AddHeader("ApiKey", _configuration.GetSection("Seguridad:ApiKey").Value);
                
                // Agregar JWT token de Azure AD
                var accessToken = await GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                }
                var response = await _client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedItem = JsonConvert.DeserializeObject<T>(response.Content);
                    return returnedItem;
                }
                string errorInfo = $"Error: Acción:DELETE { response.ResponseUri } | Info: { response.StatusCode }, {response.StatusDescription} | { response.Content }";

                throw new ApplicationException(errorInfo);
            }
            catch (Exception )
            {
                throw ;
            }
        }

        /// <summary>
        /// Put:  Borrar registro
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiEndPoint"></param>
        /// <returns></returns>
        public async Task<T> Delete<T>(T dto, string apiEndPoint)
        {
            try
            {
                var request = new RestRequest(apiEndPoint, Method.Delete);
                //request.AddJsonBody(dto);
                request.AddParameter("application/json", dto, ParameterType.RequestBody);
                request.AddHeader("ApiKey", _configuration.GetSection("Seguridad:ApiKey").Value);
                
                // Agregar JWT token de Azure AD
                var accessToken = await GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                }
                var response = await _client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedItem = JsonConvert.DeserializeObject<T>(response.Content);
                    return returnedItem;
                }
                string errorInfo = $"Error: Acción:DELETE { response.ResponseUri } | Info: { response.StatusCode }, {response.StatusDescription} | { response.Content }";

                throw new ApplicationException(errorInfo);
            }
            catch
            {
                throw;
            }
        }

    }
}
