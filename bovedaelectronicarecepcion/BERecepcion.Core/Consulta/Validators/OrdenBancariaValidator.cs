using BERecepcion.Core.Consulta.Dto;
using FluentValidation;

namespace BERecepcion.Core.Consulta.Validators;

public class OrdenBancariaValidator
    : AbstractValidator<OrdenBancariaRequest>
{
    public OrdenBancariaValidator()
    {
        RuleFor(x => x.FechaInicial)
            .LessThanOrEqualTo(x => x.FechaFinal)
            //.NotNull()
            .WithMessage("La fecha inicial debe ser menor o igual a la fecha final.");
        RuleFor(x => x.FechaFinal)
            //.NotNull()
            .GreaterThanOrEqualTo(x => x.FechaInicial)
            .WithMessage("La fecha final debe ser mayor o igual a la fecha inicial.");
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
        //RuleFor(x => x.ClaveOrganismo)
        //    .NotEmpty()
        //    .WithMessage("El campo ClaveOrganismo no puede estar vacío.");
    }
}
