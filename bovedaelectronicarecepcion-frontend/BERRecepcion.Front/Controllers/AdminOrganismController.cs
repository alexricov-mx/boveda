using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    public class AdminOrganismController : Controller
    {

        private readonly ILogger<OrganismDto> _logger;
        protected readonly IRestUtility _utility;
        protected readonly IConfiguration _configuration;
        protected readonly IGenerals _generals;

        public AdminOrganismController(IConfiguration configuration, ILogger<OrganismDto> logger, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _logger = logger;
            _utility = utility;
            _generals = generals;
        }

        [UserTypeFilter("UserTypeS")]
        public async Task<IActionResult> Index(int pageNum = 1)
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

            var totalFuturo = Convert.ToInt32(config["Adefas:FecNumFuturo"]);
            var totalPasado = Convert.ToInt32(config["Adefas:FecNumPasado"]);

            var total = Convert.ToInt32(config["FecNum"]);

            List<DataDroplist> fec = new List<DataDroplist>();

            fec.Add(new DataDroplist() { id = 0, nombre = DateTime.Now.AddYears(1).Year.ToString() });
            fec.Add(new DataDroplist() { id = 1, nombre = DateTime.Now.Year.ToString() });
            for (int i = 2, j = 1; j <= totalPasado; j++, i++)
            {
                fec.Add(new DataDroplist() { id = i, nombre = DateTime.Now.AddYears(-j).Year.ToString() });
            }

            ViewBag.ddFecha = new SelectList(fec.ToList(), "id", "nombre");

            var orgDat = await _utility.GetItem<DataResult<IEnumerable<OrganismDto>>>("Catalogos");
            ViewBag.ddOrganismo = new SelectList(orgDat.Data.ToList(), "OrganismID", "Name");


            var param = new List<CustomHttpParameter>();
            int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Adefas").Value);
            param.Add(new CustomHttpParameter("pageSize", pageSize));
            param.Add(new CustomHttpParameter("pageNum", pageNum));
            var adefas = new DataResult<IEnumerable<OrganismDto>>();

            adefas = await _utility.GetItem<DataResult<IEnumerable<OrganismDto>>>("AdminOrganism/GetOrganismAsync");
          
            return View(adefas);
        }

        public async Task<JsonResult> Editar(OrganismDto organismDto)
        {
            try
            {

                organismDto.usuarioId = _generals.User.UserID;
                var datos = await _utility.Update<OrganismDto>(organismDto, organismDto.OrganismID, "AdminOrganism");

                return Json(new { success = true, responseText = "Datos Actualizados Correctamente" });
            }
            catch (Exception exp)
            {

                return Json(new { success = false, responseText = exp.Message });
            }
        }

    }
}
