using BERecepcion.Core.Copades.Dto;
using FluentValidation;

namespace BERecepcion.Core.Copades.Validators;

public class CopadeFiltroRequestValidator
    : AbstractValidator<CopadeFiltroRequest>
{
    public CopadeFiltroRequestValidator()
    {
        RuleFor(x => x.PageNumber)
        .GreaterThanOrEqualTo(0)
        .WithMessage("El número de página debe ser mayor o igual a 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor a 0.");
        RuleFor(x => x.Search)
            .NotEmpty()
            .When(x => x.Search is not null)
            .WithMessage("El campo Search no puede estar vacío.");
    }
}
