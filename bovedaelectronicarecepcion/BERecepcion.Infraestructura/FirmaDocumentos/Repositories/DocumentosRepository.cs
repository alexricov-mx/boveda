using BERecepcion.Core.Dto;
using BERecepcion.Core.Facturas.Dto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.Instrucciones.Dto;
using BERecepcion.Infraestructura.Repositories;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BERecepcion.Infraestructura.FirmaDocumentos.Repositories
{
    public class DocumentosRepository : BaseSQLServerSqlRepository, IDocumentosRepository
    {
        private readonly IConfiguration _configuration;
        public DocumentosRepository(string cnnString, IConfiguration configuration) : base(cnnString)
        {
            _configuration = configuration;
        }

        public async Task<DataResult<ArchivoPDFDto>> GetDocumentoAPAsync(string itransport, string icte, string ianio, string ilistado_emb)
        {

            DataResult<ArchivoPDFDto> dataResult = new DataResult<ArchivoPDFDto>();

            try
            {
                string token = _configuration["WebServiceAnaliticoPagoPDF:token"];
                var client = new RestClient(_configuration["WebServiceAnaliticoPagoPDF:urlEndpoint"]);
                // client.Timeout = -1;
                var request = new RestRequest("", Method.Post);
                request.AddHeader("SOAPAction", "getAnalitico");
                request.AddHeader("Content-Type", "application/xml");
                var body = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.siic.portal"">" +
                            "<soapenv:Header/>" +
                            "<soapenv:Body>" +
                                "<ws:getAnalitico>" +
                                    $"<itransport>{itransport}</itransport>" +
                                    $"<icte>{icte}</icte>" +
                                    $"<ianio>{ianio}</ianio>" +
                                    $"<ilistado_emb>{ilistado_emb}</ilistado_emb>" +
                                    $"<token>{token}</token>" +
                                "</ws:getAnalitico>" +
                            "</soapenv:Body>" +
                            "</soapenv:Envelope>";
                request.AddParameter("application/xml", body, ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response.Content);
                    XmlNodeList elemList = doc.GetElementsByTagName("string");
                    XmlNode nodo = elemList.Item(1);
                    if (nodo != null)
                    {
                        string jsonText = JsonConvert.SerializeXmlNode(nodo);
                        var archivo = System.Text.Json.JsonSerializer.Deserialize<AnaliticoPDFDto>(jsonText);
                        ArchivoPDFDto archivoPDF = new ArchivoPDFDto()
                        {
                            ARCHIVO = archivo.@string
                        };
                        if (archivoPDF.ARCHIVO == null)
                        {
                            dataResult.Message = "No se ha encontrado el archivo.";
                            dataResult.Status = System.Net.HttpStatusCode.NotFound;
                            dataResult.Data = null;
                            return dataResult;
                        }
                        dataResult.Message = "Llamado exitoso al servicio";
                        dataResult.Status = response.StatusCode;
                        dataResult.Data = archivoPDF;
                    }
                    else
                    {
                        dataResult.Message = $"Error al obtener el documento.";
                        dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    dataResult.Message = $"Error al obtener el documento.";
                    dataResult.Status = System.Net.HttpStatusCode.BadRequest;
                }
                return dataResult;

                //ConsultaAnaliticoPDFClient consultaAnaliticoPDF = new ConsultaAnaliticoPDFClient();
                //getAnaliticoResponse resPDF = new getAnaliticoResponse();
                //resPDF = await consultaAnaliticoPDF.getAnaliticoAsync(itransport, icte, ianio, ilistado_emb, token);

                //if (!string.IsNullOrEmpty(resPDF.getAnaliticoReturn[1]))
                //{
                //    dataResult.Data.ARCHIVO = resPDF.getAnaliticoReturn[1];
                //    dataResult.Message = "Se obtuvo analitico de pago con exito";
                //    dataResult.Status = System.Net.HttpStatusCode.OK;
                //}
                //else
                //{
                //    dataResult.Message = "No se encontró el analitico de pago";
                //    dataResult.Status = System.Net.HttpStatusCode.NoContent;
                //}
                //return dataResult;
            }
            catch (Exception ex)
            {
                dataResult.Message = $"Ocurrio un problema al intentar obtener el analitico de pago {ex.Message}";
                dataResult.Status = System.Net.HttpStatusCode.InternalServerError;
                return dataResult;
            }
        }

        public async Task<DataResult<ArchivoPDFDto>> GetDocumentoAsync(string SAPOrder, string Organismo)
        {
            try
            {
                DataResult<ArchivoPDFDto> resultItem = new DataResult<ArchivoPDFDto>()
                {
                    Status = System.Net.HttpStatusCode.OK
                };
                var client = new RestClient(_configuration["WebServiceDocumento:urlEndpoint"]);
                var request = new RestRequest("", Method.Post);
                request.AddHeader("authorization", $"Basic {Utils.Encoding.Base64Encode(_configuration["WebServiceDocumento:usuario_pass"])}");
                request.AddHeader("content-type", "text/xml");
                string requestXML = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                    $"<soapenv:Header/>" +
                        $"<soapenv:Body>" +
                            $"<tem:ver2>" +
                                $"<tem:request>" +
                                    $"<tem:ID>{SAPOrder}</tem:ID>" +
                                    $"<tem:TIPO>COS</tem:TIPO>" +
                                    $"<tem:AMBIENTE>{_configuration["WebServiceDocumento:ambiente"]}</tem:AMBIENTE>" +
                                    $"<tem:ORGANISMO>{Organismo}</tem:ORGANISMO>" +
                                $"</tem:request>" +
                            $"</tem:ver2>" +
                        $"</soapenv:Body>" +
                    $"</soapenv:Envelope>";
                request.AddParameter("text/xml", requestXML, ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response.Content);
                    XmlNodeList elemList = doc.GetElementsByTagName("ARCHIVO");
                    XmlNode nodo = elemList.Item(0);
                    if (nodo != null)
                    {
                        string jsonText = JsonConvert.SerializeXmlNode(nodo);
                        var archivo = System.Text.Json.JsonSerializer.Deserialize<ArchivoPDFDto>(jsonText);
                        if (archivo.ARCHIVO == null)
                        {
                            resultItem.Message = "No se ha encontrado el archivo.";
                            resultItem.Status = System.Net.HttpStatusCode.NotFound;
                            resultItem.Data = null;
                            return resultItem;
                        }
                        resultItem.Message = "Llamado exitoso al servicio";
                        resultItem.Status = response.StatusCode;
                        resultItem.Data = archivo;
                    }
                    else
                    {
                        resultItem.Message = $"Error al obtener el documento.";
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    resultItem.Message = $"Error al obtener el documento.";
                    resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataResult<ArchivoPDFDto>> GetDocumentoProgramaPagoAsync(string PPId, string Organismo)
        {
            try
            {
                DataResult<ArchivoPDFDto> resultItem = new DataResult<ArchivoPDFDto>()
                {
                    Status = System.Net.HttpStatusCode.OK
                };
                var client = new RestClient(_configuration["WebServiceDocumento:urlEndpoint"]);
                var request = new RestRequest("", Method.Post);
                request.AddHeader("authorization", $"Basic {Utils.Encoding.Base64Encode(_configuration["WebServiceDocumento:usuario_pass"])}");
                request.AddHeader("content-type", "text/xml");
                string requestXML = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">" +
                    $"<soapenv:Header/>" +
                        $"<soapenv:Body>" +
                            $"<tem:ver2>" +
                                $"<tem:request>" +
                                    $"<tem:ID>{PPId}</tem:ID>" +
                                    $"<tem:TIPO>PP</tem:TIPO>" +
                                    $"<tem:AMBIENTE>{_configuration["WebServiceDocumento:ambiente"]}</tem:AMBIENTE>" +
                                    $"<tem:ORGANISMO>{Organismo}</tem:ORGANISMO>" +
                                $"</tem:request>" +
                            $"</tem:ver2>" +
                        $"</soapenv:Body>" +
                    $"</soapenv:Envelope>";
                request.AddParameter("text/xml", requestXML, ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response.Content);
                    XmlNodeList elemList = doc.GetElementsByTagName("ARCHIVO");
                    XmlNode nodo = elemList.Item(0);
                    if (nodo != null)
                    {
                        string jsonText = JsonConvert.SerializeXmlNode(nodo);
                        var archivo = System.Text.Json.JsonSerializer.Deserialize<ArchivoPDFDto>(jsonText);
                        if (archivo.ARCHIVO == null)
                        {
                            resultItem.Message = "No se ha encontrado el archivo.";
                            resultItem.Status = System.Net.HttpStatusCode.NotFound;
                            resultItem.Data = null;
                            return resultItem;
                        }
                        resultItem.Message = "Llamado exitoso al servicio";
                        resultItem.Status = response.StatusCode;
                        resultItem.Data = archivo;
                    }
                    else
                    {
                        resultItem.Message = $"Error al obtener el documento.";
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    resultItem.Message = $"Error al obtener el documento.";
                    resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                }
                return resultItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<RestResponse> GetDocumentoESignAsync(string paqueteId, string documentoId)
        {            
            //​/ api/Representacion/GetRepresentacionPdfAsync​/{paqueteid}​/{documentoid}
            var client = new RestClient(_configuration["eSign:Endpoint:url"]);

            var request = new RestRequest($"Representacion/GetRepresentacionPdfAsync/{paqueteId}/{documentoId}",Method.Get);
            request.AddHeader("ApiKey", _configuration.GetSection("eSign:Endpoint:Apikey").Value);
            var response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<ComprobanteDto> GetDocumentoFacturaAsync(string InvoiceId)
        {
            ComprobanteDto resultItem = new ComprobanteDto();

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@InvoiceId", InvoiceId);


                    var result = await db.QueryMultipleAsync(sql: "SP_DocumentoPDF_InvoiceXML", param: par, commandType: CommandType.StoredProcedure);

                    var apresult = await result.ReadAsync<ComprobanteDto>();
                    string datos = apresult.FirstOrDefault().OriginalXml;
                    string cfdiVersion = apresult.FirstOrDefault().CFDIVersion;

                    resultItem.OriginalXml = datos;
                    resultItem.CFDIVersion = cfdiVersion;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                string Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                var Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<ComprobanteDto> GetDocumentoNotaCreditoAsync(string NotaCreditoId)
        {
            ComprobanteDto resultItem = new ComprobanteDto();

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@NotaCreditoId", NotaCreditoId);

                    var result = await db.QueryMultipleAsync(sql: "SP_DocumentoPDF_NotaCreditoXML", param: par, commandType: CommandType.StoredProcedure);

                    var apresult = await result.ReadAsync<ComprobanteDto>();
                    string datos = apresult.FirstOrDefault().OriginalXml;
                    string cfdiVersion = apresult.FirstOrDefault().CFDIVersion;

                    //resultItem.PreFacturaXML= datos;
                    resultItem.OriginalXml = datos;
                    resultItem.CFDIVersion = cfdiVersion;
                    return resultItem;
                }
            }
            catch (Exception)
            {
                //resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                //resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<ComprobanteDto> GetDocumentoComplementoPagoAsync(string PagoUUID)
        {
            ComprobanteDto resultItem = new ComprobanteDto();

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@PagosID", new Guid(PagoUUID));


                    var result = await db.QueryMultipleAsync(sql: "SP_DocumentoPDF_ComprobanteXML", param: par, commandType: CommandType.StoredProcedure);

                    var apresult = await result.ReadAsync<ComprobanteDto>();
                    string datos = apresult.FirstOrDefault().OriginalXml;

                    //resultItem.PreFacturaXML= datos;
                    resultItem.OriginalXml = datos;
                    return resultItem;
                }
            }
            catch (Exception)
            {
                //resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                //resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }
        public async Task<PaymentListDto> GetDocumentoListaPagoAsync(string ListaPago_id)
        {
            PaymentListDto resultItem = new PaymentListDto();

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@ListaPagoid", ListaPago_id);


                    return await db.QueryFirstOrDefaultAsync<PaymentListDto>(sql: "SP_DocumentoPDF_PaymentListPDF", param: par, commandType: CommandType.StoredProcedure);

                    //resultItem.ListPayment = result;
                    //return resultItem;
                }
            }
            catch (Exception)
            {
                //resultItem.Message = $"Ocurrio un problema. Contacta a tu administrador. {ex.Message}";
                //resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }
        }

        public async Task<LogoDto> GetImagenAsync(string clave)
        {
            LogoDto resultItem = new LogoDto();

            try
            {
                //using para levantar la conexion al BD
                using (IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@clave", clave);


                    return await db.QueryFirstOrDefaultAsync<LogoDto>(sql: "SP_logocopade_Logo", param: par, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
                return resultItem;
            }
        }

    }
}
