using BERecepcion.Core.Estimaciones.Dtos;
using FluentValidation;

namespace BERecepcion.Core.Estimaciones.Validators;

public class SOEstimationProveedorPaginatorValidator
    : AbstractValidator<SOEstimationProveedorRequestDto>
{
    public SOEstimationProveedorPaginatorValidator()
    {
        RuleFor(x => x.CreditorNumber)
            .NotEmpty()
            .WithMessage("Creditor number is required.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor a 0.");
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor a 0.");
    }
}
