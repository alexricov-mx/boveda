using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IPerfilesRepository
    {
        Task<DataResult<IEnumerable<ProfilesDto>>> GetPerfilesAsync(int pageSize, int pageNum = 1, string search = null);
        Task<DataResult<IEnumerable<ProfilesDto>>> GetAllPerfilesAsync(string search);
        Task<DataResult<ProfilesDto>> GetPerfilAsync(Guid ProfileID);
        Task<DataResult<ProfilesDto>> BorraPerfilAsync(Guid ProfileID);
        Task<DataResult<ProfilesDto>> InsertaPerfilAsync(ProfilesDto dto);
        Task<DataResult<ProfilesDto>> ActualizaPerfilAsync(ProfilesDto dto);
    }
}
