using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IRestUtility utility, IGenerals generals, IConfiguration configuration)
        {
            _logger = logger;
            _utility = utility;
            _generals = generals;
            _configuration = configuration;
        }
        [ValidateUser]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (HttpContext.Session.GetString("UserMenu") == null && User.Identity.IsAuthenticated)
                {
                    var param = new List<CustomHttpParameter>();
                    param.Add(new CustomHttpParameter("Email", _generals.User.Email));
                    var loginDto = await _utility.GetItem<DataResult<UsersDto>>("/Login", param);

                    var userMenu = loginDto.Data.Profile.RolesCatalogo.GroupBy(x => x.Categoria, (key, group) => group.First()).ToList();
                    foreach (var item in userMenu)
                    {
                        item.SubMenu.AddRange(loginDto.Data.Profile.RolesCatalogo.Where(x => x.Categoria.Equals(item.Categoria)).Select(x => new SubMenuDto
                        {
                            Controller = x.Controller,
                            Rol = x.Rol,
                            Action = x.Action,
                            Descripcion = x.Descripcion
                        }));
                    }

                    HttpContext.Session.SetString("UserMenu", JsonConvert.SerializeObject(userMenu));
                    var infoAplicativo = new InfoAplicativoModel
                    {
                        Aplicativo = _configuration["infoAplicativo:Aplicativo"],
                        BuildId = _configuration["infoAplicativo:BuildId"],
                        Version = _configuration["infoAplicativo:Version"],
                        Ambiente = _configuration["infoAplicativo:Ambiente"],
                        Mensaje = _configuration["infoAplicativo:Mensaje"]
                    };
                    HttpContext.Session.SetString("InfoAplicativo", JsonConvert.SerializeObject(infoAplicativo));

                    //Bitacora de accesos
                    string descripcion = $"Inicio de sesión del usuario: {_generals.User.Name}, con correo electrónico: {_generals.User.Email}" + (!string.IsNullOrWhiteSpace(_generals.User.Token) ? string.Concat(" y ficha ", _generals.User.Token) : string.Concat(" y número de acreedor ", _generals.User.CreditorNumber));
                    DataResult<BitacoraDto> bitacora = new DataResult<BitacoraDto>
                    {
                        Data = new BitacoraDto(_generals.User.UserID, "Acceso", "Acceso", descripcion)
                    };
                    await _utility.Post(bitacora, "Bitacora/InsertaBitacoraAsync");

                }
                return View();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return RedirectToRoute(new { area = "MicrosoftIdentity", controller = "Account", action = "SignOut" });
            }
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToRoute(new { area = "MicrosoftIdentity", controller = "Account", action = "SignOut" });
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // NO limpiar cookies automáticamente aquí
            // Solo registrar el acceso a la página de error

            Serilog.Log.Warning($"Usuario accedió a página de error. Request ID: {Activity.Current?.Id ?? HttpContext.TraceIdentifier}");

            ViewBag.Message = "No pudo iniciar sesión. Por favor, intente nuevamente.";
            ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }
    }
}
