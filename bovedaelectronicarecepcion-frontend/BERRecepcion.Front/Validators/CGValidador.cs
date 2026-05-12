using BERRecepcion.Front.Models.Dto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Validators
{
    public class CGValidador: AbstractValidator<ManagementCentersDto>
    {
        public CGValidador()
        {
            RuleFor(x => x.Signer1)
                .NotEmpty().WithMessage("El campo firmante 1 no puede estar vacío.")
                .NotNull().WithMessage("El campo firmante 1 no puede estar vacío.")
                .Length(6).WithMessage("Verifica el campo firmante 1");
            RuleFor(x => x.Alternate1)
                .NotEmpty().WithMessage("El campo suplente 1 no puede estar vacío.")
                .NotNull().WithMessage("El campo suplente 1 no puede estar vacío.")
                .Length(6).WithMessage("Verifica el campo suplente 1");
            RuleFor(x => x.Signer2)
                .NotEmpty().WithMessage("El campo firmante 2 no puede estar vacío.")
                .NotNull().WithMessage("El campo firmante 2 no puede estar vacío.")
                .Length(6).WithMessage("Verifica el campo firmante 2");
            RuleFor(x => x.Alternate2)
                .NotEmpty().WithMessage("El campo suplente 2 no puede estar vacío.")
                .NotNull().WithMessage("El campo suplente 2 no puede estar vacío.")
                .Length(6).WithMessage("Verifica el campo suplente 2");           
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("El campo descripción no puede estar vacío.")
                .NotNull().WithMessage("El campo descripción no puede estar vacío.");

        }
    }
}
