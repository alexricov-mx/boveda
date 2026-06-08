using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Common.Results;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Exceptions;
using BERecepcion.Core.Interfaces;
using BERecepcion.Core.Interfaces.Auth;
using BERecepcion.Core.OrdenSurtimiento.Dto;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Core.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Services;

public class ConsultaServiceAsync(
    IConsultasRepository consultasRepository
    , ISOEstimationRepository sOEstimationRepository
    , IUsuariosRepository usuariosRepository
    , ICurrentUserService currentUserService
    )
    : IConsultaServiceAsync
{
    private readonly IConsultasRepository _consultasRepository = consultasRepository;
    private readonly ISOEstimationRepository _sOEstimationRepository = sOEstimationRepository;
    private readonly IUsuariosRepository _usuariosRepository = usuariosRepository;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<PagedResult<SOEstimationDto>>> GetEstimacionesBancariasAsync(
        EstimacionBancariaRequest request,
        CancellationToken cancellationToken = default
        )
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
            request.Search = SearchText.GetWhereClause(request.Search, ["OrganismClave", "Contract", "saporder", "CreditorNumber"]);

        var result = await _consultasRepository.GetEstimacionesBancariasAsync(request, cancellationToken);

        //return Result.Success(result);
        return Result.Success(
            new PagedResult<SOEstimationDto>(
                result.Items,
                result.TotalItems,
                result.PageNumber,
                result.PageSize
            ));
    }

    public async Task<Result<PagedResult<EstimacionObraResponseDto>>> GetEstimacionesObraAsync(
        EstimacionesObraRequest request,
        CancellationToken cancellationToken = default
        )
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            request.Search = SearchText.GetWhereClause(
               request.Search,
               ["SAPOrder", "CreditorNumber", "Contract", "OrganismClave", "DocumentType", "Currency"]);
        }



        var result = await _consultasRepository
            .GetEstimacionesObraAsyncRefactorAsync(
                request,
                cancellationToken);

        var dtos = result.Items.Select(e => new EstimacionObraResponseDto
        {
            EstimacionID = e.EstimacionID,
            OrganismID = e.OrganismID,
            Contract = e.Contract,
            DocumentType = e.DocumentType,
            SAPOrder = e.SapOrder,
            Type = e.Type,
            CreditorNumber = e.CreditorNumber,
            Total = decimal.TryParse(e.Total, out var total) ? total : 0,
            Currency = e.Currency,
            Representative = e.Representative,
            Signer = e.Signer,
            EsTRI = e.EsTRI,
            ReceptionDate = e.ReceptionDate,
            FunctionaryEmail = e.FunctionaryEmails,
            FunctionaryEmailSendDate = e.FunctionaryEmailSendDate,
            FunctionarySignDate = e.FunctionarySignDate,
            FunctionaryNotifyPemexDate = e.FunctionaryNotifyPemexDate,
            ProviderEmail = e.ProviderEmails,
            ProviderEmailSendDate = e.ProviderEmailSendDate,
            ProviderSignDate = e.ProviderSignDate,
            ProviderNotifyPemexDate = e.ProviderNotifyPemexDate,
            IsFullSigned = e.IsFullSigned,
            IsCancel = e.IsCancel,
            CancelDate = e.CancelDate,
            CancelBy = e.CancelBy,
            FunctionarySignerName = e.FunctionarySignerName,
            ProviderSignerName = e.ProviderSignerName,
            FunctionaryCancelName = e.FunctionaryCancelName,
            Clave = e.OrganismClave,
            OrganismName = e.OrganismName,
            OrganismClave = e.OrganismClave
        }).ToList();

        return Result.Success(
            new PagedResult<EstimacionObraResponseDto>(
            dtos,
            result.TotalItems,
            result.PageNumber,
            result.PageSize
        ));
    }

    public async Task<Result<PagedResult<SupplyOrderDto>>> GetOrdenesBancariasAsync(
        OrdenBancariaRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var user = await _usuariosRepository.GetUserByEmailAsync(_currentUserService.Email)
            ?? throw new NotFoundException("Usuario no encontrado");

        if (!string.IsNullOrWhiteSpace(request.Search))
            request.Search = SearchText.GetWhereClause(request.Search, ["OrganismClave", "Contract", "saporder", "CreditorNumber"]);

        var result = await _sOEstimationRepository
            .GetPagedOrdenesBancariasAsync(
                request.FechaInicial??DateTime.Now,
                request.FechaFinal??DateTime.Now ,
                request.Search,
                user.UserID.ToString(),
                request.ClaveOrganismo,
                user.CreditorNumber,
                request.PageSize,
                request.PageNumber,
                request.EsDescarga,
                cancellationToken);

        //return Result.Success(result);
        return Result.Success(
            new PagedResult<SupplyOrderDto>(
                result.Items,
                result.TotalItems,
                result.PageNumber,
                result.PageSize
            ));
    }

    public async Task<Result<PagedResult<SupplyOrderDto>>> GetOrdenesSurtimientoAsync(
        OrdenSurtimientoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
            request.Search = SearchText.GetWhereClause(request.Search,
                ["SAPOrder", "Creditor", "DocumentType", "Currency", "Clave", "Contract"]);

        var result = await _consultasRepository
            .GetOrdenesSurtimientoAsync(request, cancellationToken);

        return Result.Success(
            new PagedResult<SupplyOrderDto>(
                result.Items,
                result.TotalItems,
                result.PageNumber,
                result.PageSize
            ));

    }
}
