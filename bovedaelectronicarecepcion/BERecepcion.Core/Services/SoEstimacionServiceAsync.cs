using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Estimaciones.Dtos;
using BERecepcion.Core.Interfaces;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Services;

public class SoEstimacionServiceAsync(ISOEstimationRepository estimationRepository) 
    : ISoEstimacionServiceAsync
{
    private readonly ISOEstimationRepository _estimationRepository = estimationRepository;

    public async Task<Result<PagedResult<SOEstimationInternoDto>>> GetSOEInternoPaginationAsync(
        SOEstimationInternoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var paged = await _estimationRepository.GetSOEInternoRefactorAsync(
            request.Token, request.PageSize, request.PageNumber, cancellationToken);

        var dtos = paged.Items.Select(x => new SOEstimationInternoDto
        {
            EstimacionId = x.EstimacionID,
            OrganismId = x.OrganismID,
            Contract = x.Contract,
            SapOrder = x.SapOrder,
            DocumentType = x.DocumentType,
            Type = x.Type,
            CreditorNumber = x.CreditorNumber,
            Total = x.Total,
            Currency = x.Currency,
            Signer = x.Signer,
            FunctionarySignDate = x.FunctionarySignDate,
            FunctionaryEmailSendDate = x.FunctionaryEmailSendDate,
            ProviderSignDate = x.ProviderSignDate,
            OrganismName = x.OrganismName,
            OrganismClave = x.OrganismClave
        }).ToList();

        return Result.Success(
            new PagedResult<SOEstimationInternoDto>(
                dtos, paged.TotalItems, paged.PageNumber, paged.PageSize));
    }
}
