using System;
using BERecepcion.Api.ModelBinding.Interfaces;
using Newtonsoft.Json;
public class FromJsonAttribute : Attribute, IJsonAttribute
{
    public object TryConvert(string modelValue, Type targetType, out bool success)
    {
        var value = JsonConvert.DeserializeObject(modelValue, targetType);
        success = value != null;
        return value;
    }
}
namespace BERecepcion.Api.ModelBinding.Attributes
{
    public class FromJsonAttribute : Attribute, IJsonAttribute
    {
        public object TryConvert(string modelValue, Type targetType, out bool success)
        {
            var value = JsonConvert.DeserializeObject(modelValue, targetType);
            success = value != null;
            return value;
        }
    }
}
