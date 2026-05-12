using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Admin.Interfaces.Repositories
{
    public interface IOrganismRepository
    {
        Task<DataResult<IEnumerable<OrganismDto>>> GetOrganismAsync(/*string search = null*/);

        Task<DataResult<OrganismDto>> ActualizaOrganismAsync(OrganismDto organismDto);
    }
}
