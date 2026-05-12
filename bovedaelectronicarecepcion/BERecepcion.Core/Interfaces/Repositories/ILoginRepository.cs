using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Interfaces.Repositories
{
    public interface ILoginRepository
    {
        Task<DataResult<UsersDto>> GetUsuarioAsync(string Email);
    }
}

