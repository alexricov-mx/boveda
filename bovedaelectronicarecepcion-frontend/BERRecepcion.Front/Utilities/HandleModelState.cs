using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BERRecepcion.Front.Utilities
{
    public class HandleModelState
    {
        public static string GetErrors(ModelStateDictionary model)
        {
            string errors = "";
            foreach (var modelState in model.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors += string.Concat("- ",error.ErrorMessage + "<br>");
                }
            }
            return errors;
        }
    }
}
