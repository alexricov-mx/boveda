using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Consulta.Interfaces.Repositories
{
    public interface IConsultaCopadeRepository
    {
        Task<DataResult<IEnumerable<CopadeDto>>> GetConsultaCopadeAsync(DateTime fechaInicial, DateTime fechaFinal, string UserID, bool esDescarga, string pageSize, string search = null, int pageNum = 1);
        Task<DataResult<IEnumerable<CopadeDto>>> GetListaFiltroConsultaCopadeAsync(string CopadeID, string UserID, string pageSize, int pageNum = 1);
        Task<DataResult<ExpedienteEViewModel>> ExpedienteElectronico(string SAPOrder, Guid CopadeID);
        Task<DataResult<ExpedienteEViewModel>> Seguimiento(Guid CopadeID);
    }
}
