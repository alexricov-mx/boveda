using BERecepcion.Core.Facturas.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface ICFDIValidation40Repository
    {
        Task<List<ValidationError>> validateComprobante(IEnumerable<ValidationError> errorCatalog, List<ValidationError> validationErrors, ComprobanteBE40 comprobante, string sourceDocument, bool esNotaCredito);
    }
}
