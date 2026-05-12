using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface ICentrosGestoresRepository
    {
        Task<DataResult<IEnumerable<ManagementCentersDto>>> GetCGAsync(int pageSize, int pageNum = 1, string search = null, bool esDescarga = false);
        Task<DataResult<ManagementCentersDto>> GetCGByIdAsync(Guid ManagementCenterID); 
        Task<DataResult<ManagementCentersDto>> InsertaCGAsync(ManagementCentersDto cGDto);
        Task<DataResult<ManagementCentersDto>> ActualizaCGAsync(ManagementCentersDto cGDto);               
        Task<DataResult<ManagementCentersDto>> BorraCGAsync(Guid ManagementCenterId);
        Task<DataResult<IEnumerable<ManagementCentersDto>>> CargaCentrosGAsync(IEnumerable<ManagementCentersDto> result);
        Task<DataResult<IEnumerable<ManagementCentersDto>>> BusquedaCGUsuario(string ficha);
    }
}
