using BERRecepcion.Front.Models.Dto;
using FluentValidation;

namespace BERRecepcion.Front.Validators
{
    public class ControlInterfacesValidator : AbstractValidator<ControlInterfacesDto>
    {
        public ControlInterfacesValidator()
        {
            RuleFor(x => x.ControlInterfacesDetail.Mensaje).NotEmpty().NotNull().WithMessage("Debe especificar una justificación antes de continuar.");
        }
    }
}
