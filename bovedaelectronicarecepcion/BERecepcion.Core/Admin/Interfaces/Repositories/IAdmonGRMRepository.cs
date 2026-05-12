using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IAdmonGRMRepository
    {
        Task<DataResult<string>> InsertaFirmaGRMAsync(AdmonGRMDto admonGRMDto);

        Task<DataResult<IEnumerable<AdmonGRMDto>>> GetFirmasGRMAsync(string AdministratorToken, int tmpPageNum, int tmpPageSize);
    }
}
