using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.ExpedienteElectronico.Dto;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Services;

public class ExpedienteElectronicoServiceAsync(
    IExpedienteElectronicoRepositoryAsync expedienteElectronicoRepository)
    : IExpedienteElectronicoServiceAsync
{
    private readonly IExpedienteElectronicoRepositoryAsync _expedienteElectronicoRepository = expedienteElectronicoRepository;

    public async Task<Result<ExpedienteEViewModel>> GetExpedienteAsync(
        ExpedienteElectronicoRequest request,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await _expedienteElectronicoRepository
            .GetExpedienteElectronico(request, cancellationToken);

        return Result.Success(dataResult);
    }
}
