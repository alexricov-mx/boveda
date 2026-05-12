using BERecepcion.Core.OrdenSurtimiento.Dto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace BERecepcion.Api.Validators
{
    public class SupplyOrderKeyValidator: AbstractValidator<SupplyOrderKey>
    {
        public SupplyOrderKeyValidator()
        {
            RuleFor(x => x.SupplyOrderId).NotEmpty().NotNull();
        }
    }
}
