using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IRolesCatalogoRepository
    {
        Task<DataResult<IEnumerable<RolesCatalogoDto>>> GetRolesAsync();
    }
}
