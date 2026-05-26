using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Services;

public class ReactivaProcesoServiceAsync
    : IReactivaProcesoServiceAsync
{
    private readonly IReactivaProcesosRepository _reactivaProcesosRepository;

    public ReactivaProcesoServiceAsync(IReactivaProcesosRepository reactivaProcesosRepository)
    {
        _reactivaProcesosRepository = reactivaProcesosRepository;
    }

    public async Task<Result<IEnumerable<ReactivaProcesosResponseDto>>> GetReactivaProcessAsync(string SAPOrder, CancellationToken cancellationToken)
    {
        var result = await _reactivaProcesosRepository
            .GetReactivaProcesosBySAPOrderAsync(SAPOrder, cancellationToken);

        return Result.Success(result);
    }
}
