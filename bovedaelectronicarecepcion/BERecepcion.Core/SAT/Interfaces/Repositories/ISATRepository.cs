using BERecepcion.Core.Dto;
using BERecepcion.Core.SAT.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.SAT.Interfaces.Repositories
{
    public interface ISATRepository
    {
        Task<DataResult<ValidacionSATDto>> GetValidacionSATAsync(ValidacionSATDto validacionSAT);
        Task<DataResult<string>> validaCFDI(IFormFile archivoCFDI);
        #region Catalogos
        Task<DataResult<string>> GetDireccionAsync(string CodigoPostal, string Municipio, string Localidad, string Estado);
        #endregion
    }
}
