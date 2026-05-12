using BERRecepcion.Front.Models.Dto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Validators
{
    public class ProfilesValidator : AbstractValidator<ProfilesDto>
    {
        public ProfilesValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del perfil no debe estar vacío.")
                .NotNull().WithMessage("El nombre del perfil no debe estar vacío.")
                .MaximumLength(50).WithMessage("El nombre del perfil no debe contener más de 50 caracteres.");
            RuleFor(x => x.ProfilesRoles)
                .NotEmpty().WithMessage("Debe seleccionar por lo menos un rol al perfil.");
        }
    }
}
