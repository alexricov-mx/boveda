using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ConsultaCFDIService;
using Microsoft.AspNetCore.Http;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;
using BERecepcion.Core.Models;
using BERecepcion.Core.SAT.Dto;
using BERecepcion.Core.SAT.Interfaces.Repositories;
using BERecepcion.Infraestructura.Repositories;
using System.Net;
using System.Data;
using Dapper;

namespace BERecepcion.Infraestructura.SAT.Repositories
{
    public class SATRepository : BaseSQLServerSqlRepository, ISATRepository
    {
        public SATRepository(string cnnString) : base(cnnString)
        {
        }
        public async Task<DataResult<ValidacionSATDto>> GetValidacionSATAsync(ValidacionSATDto validacionSAT)
        {

            DataResult<ValidacionSATDto> resultItem = new DataResult<ValidacionSATDto>();

            try
            {
                ConsultaCFDIServiceClient oConsultaCFDI = new ConsultaCFDIServiceClient();
                Acuse oAcuse = new Acuse();

                string strRequest = $"?re={validacionSAT.RFCEmisor}&rr={validacionSAT.RFCReceptor}&tt={validacionSAT.Total}&id={validacionSAT.UUID}";

                oAcuse = await oConsultaCFDI.ConsultaAsync(strRequest);

                if (oAcuse != null)
                {
                    validacionSAT.CodigoEstatus = oAcuse.CodigoEstatus;
                    // ******     Mensajes de Rechazo ****** //
                    //N 601: La expresión impresa proporcionada no es válida.
                    //Este código de respuesta se presentará cuando la petición de validación no se haya
                    //respetado en el formato definido.

                    //N 602: Comprobante no encontrado.
                    //Este código de respuesta se presentará cuando el UUID del comprobante no se
                    //encuentre en la Base de Datos del SAT

                    // ****** Mensajes de Aceptación ****** //
                    //S Comprobante obtenido satisfactoriamente

                    validacionSAT.Estado = oAcuse.Estado;
                    validacionSAT.EsCancelable = oAcuse.EsCancelable;
                    validacionSAT.EstatusCancelacion = oAcuse.EstatusCancelacion;
                    validacionSAT.ValidacionEFOS = oAcuse.ValidacionEFOS;
                    resultItem.Data = validacionSAT;
                    resultItem.Message = oAcuse.CodigoEstatus;

                    if (string.IsNullOrEmpty(oAcuse.ValidacionEFOS))
                    {
                        resultItem.Status = System.Net.HttpStatusCode.NotFound;
                        resultItem.Message = "Ocurrió un error al conectar con SAT, por favor intente más tarde.";
                        return resultItem;
                    }
                    if (oAcuse.ValidacionEFOS.Equals("200") && validacionSAT.CodigoEstatus.StartsWith("S "))
                    {
                        //Este código de respuesta se presentará cuando la validación del RFC Emisor del CFDI
                        //no se encuentre dentro de la lista de Empresa que Factura Operaciones Simuladas (EFOS)
                        resultItem.Status = System.Net.HttpStatusCode.OK;
                        return resultItem;
                    }
                    if (oAcuse.ValidacionEFOS.Equals("200") && !validacionSAT.CodigoEstatus.StartsWith("S "))
                    {
                        resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                        return resultItem;
                    }
                    if (oAcuse.ValidacionEFOS.Equals("100"))
                    {
                        //Este código de respuesta se presentará cuando la validación del RFC Emisor del CFDI
                        //se encuentre dentro de la lista de Empresa que Factura Operaciones Simuladas (EFOS) 
                        resultItem.Status = System.Net.HttpStatusCode.Continue;
                        return resultItem;
                    }
                }
                else
                {
                    resultItem.Message = "Consulta Vacia";
                    resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                }
                return resultItem;
            }
            catch (Exception ex)
            {

                resultItem.Message = $"Ocurrio un problema con la consulta {ex.Message}";
                resultItem.Status = System.Net.HttpStatusCode.BadRequest;
                return resultItem;
            }

        }

        public async Task<DataResult<string>> validaCFDI(IFormFile archivoCFDI)
        {
            DataResult<string> response = new DataResult<string>()
            {
                Status = System.Net.HttpStatusCode.OK,
                Message = "Validacion"
            };

            var resultXML = new StringBuilder();
            Comprobante Comprobante = new Comprobante();
            using (var reader = new StreamReader(archivoCFDI.OpenReadStream()))
            {
                var serializer = new XmlSerializer(typeof(Comprobante));
                Comprobante = (Comprobante)serializer.Deserialize(reader);
            }
            XmlDocument doc = new XmlDocument();

            foreach (var item in Comprobante.Addenda.Any)
            {



                XmlNodeList nodoAddenda = item.ChildNodes;
                foreach (XmlNode nodoItem in nodoAddenda)
                {
                    if (nodoItem != null)
                    {
                        string jsonText = JsonConvert.SerializeXmlNode(nodoItem);
                        jsonText = jsonText.Replace("pm:", "");
                        Addenda_item oAddenda = System.Text.Json.JsonSerializer.Deserialize<Addenda_item>(jsonText);
                    }
                }
            }


            ///////////////////////////


            //var obj = new MyElement() { Content = "testing" };
            //var namespaces = new XmlSerializerNamespaces();
            //namespaces.Add("xsi", MyElement.SchemaInstanceNamespace);
            //namespaces.Add("myns", MyElement.ElementNamespace);
            //var serializer = new XmlSerializer(typeof(MyElement));
            //using (var writer = File.CreateText("serialized.xml"))
            //{
            //    serializer.Serialize(writer, obj, namespaces);
            //}



            //////////////////



            response.Data = "";
            await Task.CompletedTask;
            return response;
        }

        public static byte[] StreamToByteArray(Stream inputStream)
        {
            byte[] bytes = new byte[16384];
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count;
                while ((count = inputStream.Read(bytes, 0, bytes.Length)) > 0)
                {
                    memoryStream.Write(bytes, 0, count);
                }
                return memoryStream.ToArray();
            }
        }

        public static Stream ByteArrayToStream(byte[] InBytes)
        {
            byte[] bytes = new byte[16384];
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count = 0;
                while ((count = InBytes[count]) > 0)
                {
                    memoryStream.Write(bytes, 0, count++);
                }
                return memoryStream;
            }
        }
        #region Catalogos
        public async Task<DataResult<string>> GetDireccionAsync(string CodigoPostal, string Municipio, string Localidad, string Estado)
        {
            DataResult<string> resultItem = new DataResult<string>()
            {
                Status = HttpStatusCode.OK
            };
            try
            {
                using(IDbConnection db = GetConnection())
                {
                    DynamicParameters par = new DynamicParameters();
                    par.Add("@c_CodigoPostal", CodigoPostal);
                    par.Add("@c_Municipio", Municipio);
                    par.Add("@c_Localidad", Localidad);
                    par.Add("@c_Estado", Estado);

                    var result = await db.QueryFirstOrDefaultAsync<string>(sql: "SP_c_SAT_GetDireccion", param: par, commandType: CommandType.StoredProcedure);
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        resultItem.Status = HttpStatusCode.BadRequest;
                        resultItem.Message = "Ocurrió un problema al obtener la dirección, favor de intentar más tarde.";
                        return resultItem;
                    }
                    resultItem.Data = result;
                    return resultItem;
                }
            }
            catch (Exception ex)
            {
                resultItem.Status = HttpStatusCode.BadRequest;
                resultItem.Message = $"Ocurrio un problema: {ex.Message}";
                return resultItem;
            }
        }
        #endregion
    }
}
