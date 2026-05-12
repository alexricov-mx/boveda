using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BERecepcion.Core.Models;
using BERecepcion.Core.Facturas.Dto;

namespace BERecepcion.Core.Facturas.Interfaces.Repositories
{
    public interface IRecepcionElectronicaPRepository
    {
        Task<DataResult<RecepcionElectronicaPDto>> SaveInvoiceREP(RecepcionElectronicaPDto dto);
        Task<DataResult<RecepcionElectronicaPDto>> SavePagoREP(RecepcionElectronicaPDto dto);
    }
}
