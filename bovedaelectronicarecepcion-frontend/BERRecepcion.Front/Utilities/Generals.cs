using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Serilog;

namespace BERRecepcion.Front.Utilities
{
    public class Generals : IGenerals
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService _currentUserService;

        public Generals(IHttpContextAccessor httpContextAccessor, ICurrentUserService currentUserService)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Obtiene el usuario autenticado actual.
        /// REFACTOR: Ahora delega a ICurrentUserService para aprovechar caché y null-safety.
        /// </summary>
        public UsersDto User => _currentUserService.GetCurrentUser();

        /// <summary>
        /// Crea un DTO de usuario legacy. Requiere que User no sea null.
        /// </summary>
        public UsuarioDto Usuario
        {
            get
            {
                var user = User;
                if (user == null)
                {
                    throw new InvalidOperationException("No hay usuario autenticado. Verifica [Authorize] o ValidateUser antes de acceder a Usuario.");
                }

                return new UsuarioDto
                {
                    Rfc = user.RFC,
                    Ficha = user.UserType.Equals("UserTypeP") ? 0 : int.Parse(user.Token),
                    Nombre = user.Name,
                    Correo = user.Email
                };
            }
        }

        public byte[] GetBytesFromFile(IFormFile file)
        {
            byte[] result;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                result = ms.ToArray();
            }
            return result;
        }

        private async Task<string> GetFileVersion(IFormFile factura)
        {
            string version = "3.3";

            try
            {
                using (var readerPre = new StreamReader(factura.OpenReadStream()))
                {
                    string data = await readerPre.ReadToEndAsync();
                    if (!String.IsNullOrEmpty(data))
                    {
                        data = data.Replace("\"", "'").Replace(" ", "").Replace("\n", "").Replace("\t", "").ToUpper();
                        int i = data.IndexOf("VERSION=");
                        if (i >= 0)
                        {
                            i = i + 9;
                            bool found = false;
                            for (int ii = 0; ii < 5 && ii < data.Length && !found; ii++)
                                if (data.Substring(i + ii, 1) == "'")
                                {
                                    found = true;
                                    version = data.Substring(i, ii);
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return version;
        }

        private ComprobanteBEDeserialized GetDeserialized(StreamReader reader, string version)
        {
            ComprobanteBEDeserialized result = new ComprobanteBEDeserialized();

            if (version == "3.3")
            {
                var serializer = new XmlSerializer(typeof(ComprobanteBE));
                var _comprobante = (ComprobanteBE)serializer.Deserialize(reader);
                result.ComprobanteBE = _comprobante;
                result.ComprobanteOriginal = string.Empty;
                return result;
            }
            else
            {
                if (version == "4.0")
                {
                    var serializer = new XmlSerializer(typeof(ComprobanteBE40));
                    var _comprobante = (ComprobanteBE40)serializer.Deserialize(reader);
                    ComprobanteBE _comprobanteBE = ComprobanteConvert.ComprobanteBE40ToComprobanteBE(_comprobante);
                    result.ComprobanteBE = _comprobanteBE;
                    result.ComprobanteOriginal = JsonConvert.SerializeObject(_comprobante);
                    return result;
                }
                else
                {
                    //default: version 3.3
                    var serializer = new XmlSerializer(typeof(ComprobanteBE));
                    var _comprobante = (ComprobanteBE)serializer.Deserialize(reader);
                    result.ComprobanteBE = _comprobante;
                    result.ComprobanteOriginal = string.Empty;
                    return result;
                }
            }
        }

        public async Task<DataResult<ComprobanteBEDeserialized>> GetComprobante(IFormFile factura, bool? esFactura = null, bool? esNotaCredito = null, bool? esRecepcionEP = null)
        {
            DataResult<ComprobanteBEDeserialized> result = new DataResult<ComprobanteBEDeserialized>
            { Status = System.Net.HttpStatusCode.OK };
            string version = "3.3";
            try
            {
                version = await GetFileVersion(factura);

                using (var reader = new StreamReader(factura.OpenReadStream()))
                {
                    ComprobanteBEDeserialized _comprobante = GetDeserialized(reader, version);
                    if (_comprobante.ComprobanteBE.Addenda != null && _comprobante.ComprobanteBE.Addenda.Addenda_Pemex == null)
                    {
                        var xmlFactura = await ReadFileAsync(factura);
                        XElement xmlAddenda = XElement.Parse(xmlFactura);
                        var _nodoAddenda = (from el in xmlAddenda.Descendants() where el.Name == "Addenda_Pemex" select el).FirstOrDefault();
                        if (_nodoAddenda != null)
                        {
                            var addenda = DeserializeAddenda(_nodoAddenda.ToString());
                            if (addenda.Status == System.Net.HttpStatusCode.OK)
                                _comprobante.ComprobanteBE.Addenda = addenda.Data;
                        }
                    }
                    if (esFactura != null && esFactura == true && _comprobante.ComprobanteBE.TipoDeComprobante != "I")
                    {
                        result.Status = System.Net.HttpStatusCode.BadRequest;
                        result.Message = "El archivo seleccionado no corresponde a una factura.";
                        return result;
                    }
                    if (esNotaCredito != null && esNotaCredito == true && _comprobante.ComprobanteBE.TipoDeComprobante != "E")
                    {
                        result.Status = System.Net.HttpStatusCode.BadRequest;
                        result.Message = "El archivo seleccionado no corresponde a una nota de crédito.";
                        return result;
                    }
                    if (esRecepcionEP != null && esRecepcionEP == true && _comprobante.ComprobanteBE.TipoDeComprobante != "P")
                    {
                        result.Status = System.Net.HttpStatusCode.BadRequest;
                        result.Message = "El archivo seleccionado no corresponde a un Complemento de Pago.";
                        return result;
                    }
                    result.Data = _comprobante;
                }
                return result;
            }
            catch (Exception)
            {
                result.Status = System.Net.HttpStatusCode.BadRequest;
                result.Message = "Ocurrió un error al obtener los datos de la factura, verifica tu archivo xml";
                return result;
            }
        }
        public async Task<DataResult<List<ComprobanteBE>>> GetComprobantes(IEnumerable<IFormFile> facturas, bool? esFactura = null, bool? esNotaCredito = null)
        {
            DataResult<List<ComprobanteBE>> result = new DataResult<List<ComprobanteBE>>
            { Status = System.Net.HttpStatusCode.OK, Data = new List<ComprobanteBE>() };
            try
            {
                var serializer = new XmlSerializer(typeof(ComprobanteBE));
                foreach (var item in facturas)
                {
                    using (var reader = new StreamReader(item.OpenReadStream()))
                    {
                        var _comprobante = (ComprobanteBE)serializer.Deserialize(reader);
                        _comprobante.FileName = item.FileName;
                        if (_comprobante.Addenda != null && _comprobante.Addenda.Addenda_Pemex == null)
                        {
                            var xmlFactura = await ReadFileAsync(item);
                            XElement xmlAddenda = XElement.Parse(xmlFactura);
                            var _nodoAddenda = (from el in xmlAddenda.Descendants() where el.Name == "Addenda_Pemex" select el).FirstOrDefault();
                            if (_nodoAddenda != null)
                            {
                                var addenda = DeserializeAddenda(_nodoAddenda.ToString());
                                if (addenda.Status == System.Net.HttpStatusCode.OK)
                                    _comprobante.Addenda = addenda.Data;
                            }
                        }
                        if (esFactura != null && esFactura == true && _comprobante.TipoDeComprobante != "I")
                        {
                            result.Status = System.Net.HttpStatusCode.BadRequest;
                            result.Message = "Alguno de los archivos seleccionados no corresponden a una factura.";
                            return await Task.Run(() => result);
                        }
                        if (esNotaCredito != null && esNotaCredito == true && _comprobante.TipoDeComprobante != "E")
                        {
                            result.Status = System.Net.HttpStatusCode.BadRequest;
                            result.Message = "Alguno de los archivos seleccionados no corresponden a una nota de crédito.";
                            return await Task.Run(() => result);
                        }
                        
                        result.Data.Add(_comprobante);  
                    }
                }
                return await Task.Run(() => result);
            }
            catch (Exception)
            {
                result.Status = System.Net.HttpStatusCode.BadRequest;
                result.Message = "Ocurrió un error al obtener los datos de alguno de los archivos seleccionados, favor de verificar";
                return await Task.Run(() => result);
            }
        }
        public DataResult<Addenda> DeserializeAddenda(string xmlAddenda)
        {
            DataResult<Addenda> result = new DataResult<Addenda> { Status = System.Net.HttpStatusCode.OK };
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlAddenda);
                XmlElement _addenda = doc.DocumentElement;
                _addenda.RemoveAllAttributes();
                var jsonAddenda = JsonConvert.SerializeXmlNode(_addenda);
                var addenda = JsonConvert.DeserializeObject<Addenda>(jsonAddenda);
                result.Data = addenda;
                return result;
            }
            catch (Exception)
            {
                result.Status = System.Net.HttpStatusCode.BadRequest;
                result.Message = "La addenda no tiene el formato correcto, favor de revisar.";
                return result;
            }
        }
        public async Task<string> ReadFileAsync(IFormFile file)
        {
            try
            {
                var result = new StringBuilder();
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    while (reader.Peek() >= 0)
                        result.AppendLine(await reader.ReadLineAsync());
                }
                return result.ToString();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public DataResult<ComprobanteBE> GetComprobante1(Stream factura, bool? esFactura = null)
        {
            DataResult<ComprobanteBE> result = new DataResult<ComprobanteBE>
            { Status = System.Net.HttpStatusCode.OK };
            try
            {
                var serializer = new XmlSerializer(typeof(ComprobanteBE));
                using (var reader = new StreamReader(factura))
                {
                    var _comprobante = (ComprobanteBE)serializer.Deserialize(reader);

                    result.Data = _comprobante;
                }
                return result;
            }
            catch (Exception)
            {
                result.Status = System.Net.HttpStatusCode.BadRequest;
                result.Message = "Ocurrió un error al obtener los datos de la factura";
                return result;
            }
        }
        public string GenerateQRCode(string value)
        {
            QRCodeGenerator qRCodeGenerador = new QRCodeGenerator();
            QRCodeData qRCodeData = qRCodeGenerador.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qRCodeData);
            Bitmap bitmap = qrCode.GetGraphic(15);
            byte[] arrBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                arrBytes = ms.ToArray();
            }
            return Convert.ToBase64String(arrBytes);
        }
        public string RemoveSpecialCharacters(string str)
        {
            str = str.ToLower().Replace("insert", "").Replace("into","").Replace("values","").Replace("create", "").Replace("drop", "").Replace("database","").Replace("alter","");
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == ',')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public string GetStringSearch(IEnumerable<string> search = null)
        {
            if (search == null || !search.Any()) return string.Empty;

            List<string> cleanedSearchList = new List<string>();
            foreach (var item in search)
            {
                string removed = RemoveSpecialCharacters(item);
                cleanedSearchList.Add(removed);
            }

            return Regex.Replace(string.Join(";", cleanedSearchList), " *, *", ",");
        }
    }
}