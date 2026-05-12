using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface ICFDIValidationCartaPorteRepository
    {
        Task<List<ValidationError>> validateCartaPorte(IEnumerable<ValidationError> catalogErrors, List<ValidationError> validationErrors, CartaPorte cartaPorte, string documento, bool esNotaCredito);
    }
}
