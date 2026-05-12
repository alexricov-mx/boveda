using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    public class ConstructionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
