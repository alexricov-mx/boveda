using BERecepcion.Core.OrdenSurtimiento.Dto;
using FluentValidation;

namespace BERecepcion.Core.OrdenSurtimiento.Validators;

public class SupplyOrderPagedRequestValidator
    : AbstractValidator<SupplyOrderPagedRequest>
{
    public SupplyOrderPagedRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor a 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor a 0.");
        RuleFor(x => x.Search)
            .NotEmpty()
            .When(x => x.Search is not null)
            .WithMessage("El campo Search no puede estar vacío.");
    }
}
