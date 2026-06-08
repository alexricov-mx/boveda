using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Copades.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Copades.Interfaces.Repositories
{
    public interface ICopadeRepository
    {
        Task<DataResult<IEnumerable<CopadeDto>>> GetListaFiltroCopadesAsync(string UserID, int pageSize, int pageNum = 1, string search = null);

        Task<PagedResult<CopadeDto>> GetPagedFiltroCopadesAsync(
            string userID,
            int pageSize,
            int pageNum,
            string search,
            CancellationToken cancellationToken = default);

        #region
        Task<DataResult<CopadeDto>> CopadeFirmaAsync(Guid CopadeID, string Token);
        Task<DataResult<CopadeDto>> CopadeFirmaCorreoAsync(Guid CopadeID, string Email, string Paso);
        Task<DataResult<ExternosDto>> PostFirmaAsync(ExternosDto externosDto);
        Task<DataResult<ExternosDto>> PostDocumentoAsync(ExternosDto externosCollection, IFormFile documentoPDF);
        Task<DataResult<IEnumerable<UsersDto>>> GetSignersByCopadeID(Guid CopadeID);
        Task<DataResult<SignersDto>> GetSigners2ByCopadeID(Guid CopadeID);
        #endregion

        Task<DataResult<IEnumerable<CopadeDto>>> GetListaCopadeBancarioAsync(DateTime start, DateTime end, string search, string UserID, string claveOrganismo, string creditorNumber, int pageSize, int pageNum = 1, bool esDescarga = false);
    }
}
