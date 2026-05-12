using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Interfaces
{
    public interface IGenerals
    {
        UsersDto User { get; }
        UsuarioDto Usuario { get; }
        byte[] GetBytesFromFile(IFormFile file);
        Task<DataResult<ComprobanteBEDeserialized>> GetComprobante(IFormFile factura, bool? esFactura = null, bool? esNotaCredito = null, bool? esRecepcionEP = null);
        Task<string> ReadFileAsync(IFormFile file);
        Task<DataResult<List<ComprobanteBE>>> GetComprobantes(IEnumerable<IFormFile> facturas, bool? esFactura = null, bool? esNotaCredito = null);
        public DataResult<Addenda> DeserializeAddenda(string xmlAddenda);
        DataResult<ComprobanteBE> GetComprobante1(Stream factura, bool? esFactura = null);
        public string GenerateQRCode(string value);
        public string RemoveSpecialCharacters(string str);
    }
}
