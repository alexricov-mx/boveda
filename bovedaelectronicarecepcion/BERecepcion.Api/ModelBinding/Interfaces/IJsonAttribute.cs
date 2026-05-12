using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.ModelBinding.Interfaces
{
    public interface IJsonAttribute
    {
        object TryConvert(string modelValue, Type targertType, out bool success);
    }
}
