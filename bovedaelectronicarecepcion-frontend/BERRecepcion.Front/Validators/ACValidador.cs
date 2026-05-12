using BERRecepcion.Front.Models.Dto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Validators
{
    public class ACValidador:AbstractValidator<AltaContratosDto>
    {
        public ACValidador()
        {
            RuleFor(x => x.Contract)
                .NotEmpty().WithMessage("El campo contrato no puede estar vacio.")
                .Length(10).WithMessage("La longitud del contrato debe ser de 10 dígitos.");
            RuleFor(x => x.Firma1)
                .NotEmpty().WithMessage("El campo firmante 1 no puede estar vacio.")
                .NotNull().WithMessage("El campo firmante 1 no puede estar vacio.")
                .Length(6).WithMessage("Verifica el campo firmante 1");
            RuleFor(x => x.Firma2)
                .NotEmpty().WithMessage("El campo firmante 2 no puede estar vacio.")
                .NotNull().WithMessage("El campo firmante 2 no puede estar vacio.")
                .Length(6).WithMessage("Verifica el campo firmante 2");
            RuleFor(x => x.Suplente1)
                .NotEmpty().WithMessage("El campo suplente 1 no puede estar vacio.")
                .NotNull().WithMessage("El campo suplente 1 no puede estar vacio.")
                .Length(6).WithMessage("Verifica el campo suplente 1");
            RuleFor(x => x.Suplente2)
                .NotEmpty().WithMessage("El campo suplente 2 no puede estar vacio.")
                .NotNull().WithMessage("El campo suplente 2 no puede estar vacio.")
                .Length(6).WithMessage("Verifica el campo suplente 2");
        }
    }
}
