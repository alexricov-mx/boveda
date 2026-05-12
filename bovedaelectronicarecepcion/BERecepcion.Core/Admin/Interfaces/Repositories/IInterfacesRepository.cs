using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IInterfacesRepository
    {
        Task<DataResult<IEnumerable<ControlInterfacesDto>>> GetInterfacesAsync();
        Task<DataResult<ControlInterfacesDto>> GetControlInterfacesRolesAsync(Guid sapId);
        Task<DataResult<ControlInterfacesDto>> ActualizarAsync(ControlInterfacesDto dto);
    }
}
