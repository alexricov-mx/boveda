using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    public class ErrorController : Controller
    {
        public async Task<IActionResult> Blocked()
        {
            await HttpContext.SignOutAsync();
            return View();
        }
        public async Task<IActionResult> Deleted()
        {
            await HttpContext.SignOutAsync();
            return View();
        }
        public async Task<IActionResult> NoPermissions()
        {
            await HttpContext.SignOutAsync();
            return View();
        }
        public async Task<IActionResult> Maintenance()
        {
            await HttpContext.SignOutAsync();
            return View();
        }
        public async Task<IActionResult> Disabled()
        {
            return View();
        }
    }
}
