using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Filters
{
    public class RoleFilterAttribute : Attribute, IAsyncActionFilter
    {
        private List<string> _roles { get; set; }
        public RoleFilterAttribute(string Roles)
        {
            _roles = Roles.Split(',').ToList();
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.FindFirst("Roles") == null || _roles.Count == 0)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "Disabled" }));
                return;
            }
            var roles = context.HttpContext.User.FindFirst("Roles").Value.Split(",").ToList();

            if (roles.Count == 0)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "Disabled" }));
                return;
            }
            if(!roles.Any(x => _roles.Any(y => y == x)))
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "Disabled" }));
                return;
            }
            await next();
        }
    }
}
