using BERecepcion.Core.Estimaciones.Dtos;
using FluentValidation;

namespace BERecepcion.Core.Estimaciones.Validators;

public class GetSOEInternoPaginationValidator
    : AbstractValidator<SOEstimationInternoRequestDto>
{
    public GetSOEInternoPaginationValidator()
    {
        RuleFor(x => x.Token)
            .NotNull()
            .NotEmpty()
            .WithMessage("El token es requerido.");
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El número de página debe ser mayor o igual a 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor a 0.");
    }
}
