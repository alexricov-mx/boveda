using System;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BERRecepcion.Front.Filters
{
    public class UserTypeFilterAttribute : Attribute, IAsyncActionFilter
    {
        private List<string> _userType { get; set; }
        public UserTypeFilterAttribute(string UserType)
        {
            _userType = UserType.Split(',').ToList();
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(context.HttpContext.User.FindFirst("User") == null || _userType.Count == 0)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "Disabled" }));
                return;
            }
            var userData = JsonConvert.DeserializeObject<UsersDto>(context.HttpContext.User.FindFirst("User").Value);

            if (string.IsNullOrWhiteSpace(userData.UserType))
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "Disabled" }));
                return;
            }
            if (!_userType.Contains(userData.UserType))
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "Disabled" }));
                return;
            }
            await next();
        }
    }    
}
