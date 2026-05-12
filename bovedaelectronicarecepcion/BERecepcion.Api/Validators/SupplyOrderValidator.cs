using BERecepcion.Core.OrdenSurtimiento.Dto;
using FluentValidation;
using FluentValidation.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.Validators
{
    public class SupplyOrderValidator:AbstractValidator<SupplyOrder>
    {
        public SupplyOrderValidator()
        {
            RuleFor(x => x.OrganismId).NotEmpty().NotNull();
            RuleFor(x => x.Contract).NotNull().Length(10);
            RuleFor(x => x.DocumentType).NotNull().Length(1);
            RuleFor(x => x.SapOrder).NotNull().Length(10);
            RuleFor(x => x.Type).NotEmpty().NotNull();
            RuleFor(x => x.Creditor).NotEmpty().NotNull();
            RuleFor(x => x.CreditorNumber).NotEmpty().NotNull();
            RuleFor(x => x.CreditorRfc).NotNull().Length(13);
            RuleFor(x => x.Total).NotEmpty().NotNull();
            RuleFor(x => x.Currency).NotEmpty().NotNull();
            RuleFor(x => x.MadeBy).NotEmpty().NotNull();
            RuleFor(x => x.Representative).NotEmpty().NotNull();
            RuleFor(x => x.Signer).NotEmpty().NotNull();
            //RuleFor(x => x.FechaActividad).NotEmpty().Must(EsFechaActividadValida).WithMessage("Fecha actividad no puede ser mayor a la fecha actual");
            //RuleFor(x => x.DuracionHoras).NotNull().InclusiveBetween(0, 23).WithMessage("Las horas deben estar entre 0 y 23 horas");
            //RuleFor(x => x.DuracionMinutos).NotNull().InclusiveBetween(0, 59).WithMessage("Los minitos deben estar entre 0 y 59 minutos");​
        }

        private bool EsFechaActividadValida(DateTime fecha)
        {
            if (fecha > DateTime.Now)
            {
                return false;
            }
            return true;
        }
    }
}
