using BERecepcion.Core.Catalogos.Dto;
using BERecepcion.Core.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BERecepcion.Core.Catalogos.Interfaces.Repositories
{
    public interface ICartaPorteRepository
    {
        Task<DataResult<IEnumerable<CatalogosCartaPorteDto>>> SeleccionarAsync(string tableName, int pageNumber = 1, int pageSize = 10, string filtroBusqueda = null);

        Task<DataResult<CatalogosCartaPorteDto>> CargarAsync(DataResult<CatalogosCartaPorteDto> cartaPorte);

        Task<DataResult<CatalogosCartaPorteDto>> ValidarAsync(DataResult<CatalogosCartaPorteDto> dataResult);

        Task<DataResult<CatalogosCartaPorteDto>> AceptarAsync(DataResult<CatalogosCartaPorteDto> dataResult);

        Task<DataResult<IEnumerable<CatalogosCartaPorteDto>>> BitacoraSeleccionarAsync(int pageNumber = 1, int pageSize = 10, bool esDescarga = false);

        Task<DataResult<CatalogosCartaPorteDto>> EnviarCorreo(DataResult<CatalogosCartaPorteDto> dataResult, string mailBody);
    }
}
