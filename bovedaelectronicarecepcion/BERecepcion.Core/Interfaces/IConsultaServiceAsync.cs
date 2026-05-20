using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Dto;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Interfaces;

public interface IConsultaServiceAsync
{
    Task<Result<PagedResult<EstimacionObraResponseDto>>> GetEstimacionesObraAsync(
        EstimacionesObraRequest request,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<SupplyOrderDto>>> GetOrdenesSurtimientoAsync(
        OrdenSurtimientoRequest request,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<SOEstimationDto>>> GetEstimacionesBancariasAsync(
        EstimacionBancariaRequest request,
        CancellationToken cancellationToken = default);
}
