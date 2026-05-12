using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BERRecepcion.Front.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting.Internal;
using DocumentFormat.OpenXml.Wordprocessing;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Bibliography;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class CatalogosController : Controller
    {
        private readonly ILogger<CatalogosController> _logger;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        protected readonly IConfiguration _configuration;

        public CatalogosController(ILogger<CatalogosController> logger, IRestUtility utility, IGenerals generals, IConfiguration configuration)
        {
            _logger = logger;
            _utility = utility;
            _generals = generals;
            _configuration = configuration;
        }

        /// <summary>
        /// Catálogos Carta Porte finales
        /// </summary>
        /// <returns>Catálogos Carta Porte finales</returns>
        [HttpGet]
        [RoleFilter(Roles: "AdministrationCartaPorte")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Mantenimiento de Catálogos Carta Porte de paso
        /// </summary>
        /// <returns>Catálogos Carta Porte de paso</returns>
        [HttpGet]
        [RoleFilter(Roles: "AdministrationMaintenanceCartaPorte")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public IActionResult Maintenance()
        {
            return View();
        }

        /// <summary>
        /// Bitácora de Catálogos Carta Porte
        /// </summary>
        /// <returns>Bitácora de Catálogos Carta Porte</returns>
        [HttpGet]
        [RoleFilter(Roles: "AdministrationMaintenanceCartaPorte")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public IActionResult Bitacora()
        {
            return View();
        }

        /// <summary>
        /// Select a las tablas de Catálogos Carta Porte
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="pageNumber">Numero de la pagina</param>
        /// <param name="filtroBusqueda">Por si se desea implementar la busqueda</param>
        /// <returns>Tabla a consultar</returns>
        [HttpGet]
        [RoleFilter(Roles: "AdministrationMaintenanceCartaPorte")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public async Task<IActionResult> Seleccionar(string tableName, int pageNumber = 1, string filtroBusqueda = null)
        {
            try
            {
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:CatalogosCartaPorte").Value);

                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("tableName", tableName));
                param.Add(new CustomHttpParameter("pageNumber", pageNumber));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("filtroBusqueda", filtroBusqueda));

                var result = await _utility.GetItem<DataResult<IEnumerable<CatalogosCartaPorteDto>>>("Catalogos/SeleccionarAsync", param);

                if (result.Status != System.Net.HttpStatusCode.OK)
                {
                    return Json(new { success = false, message = "Ocurrió un error al seleccionar los Catálogos Carta Porte, por favor intente mas tarde." });
                }

                if (result.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    result.Pager = new Pager(result?.Data?.ToList().Count ?? 0, pageNumber, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={result.Pager.TotalItems}");
                    result.Pager = new Pager(result.Pager.TotalItems, pageNumber, pageSize);
                }

                ViewBag.Catalogo = tableName;

                return PartialView("_Catalogo", result);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al seleccionar los Catálogos Carta Porte, por favor intente mas tarde." });
            }
        }

        /// <summary>
        /// Select a la tabla de Bitacora Catálogos Carta Porte
        /// </summary>
        /// <param name="pageNumber">Numero de la pagina</param>
        /// <returns>Tabla de bitacora</returns>
        [HttpGet]
        [RoleFilter(Roles: "AdministrationMaintenanceCartaPorte")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public async Task<IActionResult> BitacoraSeleccionar(int pageNumber = 1)
        {
            try
            {
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:CatalogosCartaPorteBitacora").Value);

                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("pageNumber", pageNumber));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("esDescarga", false));


                var result = await _utility.GetItem<DataResult<IEnumerable<CatalogosCartaPorteDto>>>("Catalogos/BitacoraSeleccionarAsync", param);

                if (result.Status != System.Net.HttpStatusCode.OK)
                {
                    return Json(new { success = false, message = "Ocurrió un error al seleccionar los Catálogos Carta Porte, por favor intente mas tarde." });
                }

                if (result.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    result.Pager = new Pager(result?.Data?.ToList()?.Count ?? 0, pageNumber, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={result.Pager.TotalItems}");
                    result.Pager = new Pager(result.Pager.TotalItems, pageNumber, pageSize);
                }

                return PartialView("_Bitacora", result);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al seleccionar los Catálogos Carta Porte, por favor intente mas tarde." });
            }
        }

        /// <summary>
        /// Insert a las tablas Catálogos Carta Porte de paso
        /// </summary>
        /// <param name="formFile">Archivo XLS</param>
        /// <returns>Estado del proceso</returns>
        [HttpPost]
        [RoleFilter(Roles: "AdministrationMaintenanceCartaPorte")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public async Task<IActionResult> Cargar(IFormFile formFile)
        {
            try
            {
                if (formFile == null)
                {
                    return Json(new { success = false, message = "Seleccione un archivo válido." });
                }

                CatalogosCartaPorteDto cartaPorteDto = new CatalogosCartaPorteDto();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await formFile.CopyToAsync(memoryStream);

                    cartaPorteDto.ArchivoXLS = memoryStream.ToArray();
                }

                DataResult<CatalogosCartaPorteDto> dataResult = new DataResult<CatalogosCartaPorteDto>()
                {
                    Data = cartaPorteDto,
                    User = _generals.User
                };

                //DataResult<CatalogosCartaPorteDto> result = await _utility.Post(dataResult, "Catalogos/CargarAsync");
                _utility.Post(dataResult, "Catalogos/CargarAsync");

                //return Json(new { success = result.Status == System.Net.HttpStatusCode.OK, message = result.Message });
                return Json(new { success = true, message = "Su catálogo se envió correctamente, se le enviará un correo electrónico cuando concluya su procesamiento." });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al cargar los Catálogos Carta Porte, por favor intente mas tarde." });
            }
        }

        /// <summary>
        /// Valida las tablas Catálogos Carta Porte de paso
        /// </summary>
        /// <returns>Estado de la validación</returns>
        [HttpPost]
        [RoleFilter(Roles: "AdministrationMaintenanceCartaPorte")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public async Task<IActionResult> Validar()
        {
            try
            {
                DataResult<CatalogosCartaPorteDto> dataResult = new DataResult<CatalogosCartaPorteDto>();
                dataResult.Data = new CatalogosCartaPorteDto();
                dataResult.Data.isValid = false;

                DataResult<CatalogosCartaPorteDto> result = await _utility.Post(dataResult, "Catalogos/ValidarAsync");

                return Json(new { success = result.Status == System.Net.HttpStatusCode.OK, message = result.Message, isValid = result.Data.isValid });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al aceptar los Catálogos Carta Porte, por favor intente mas tarde." });
            }
        }

        /// <summary>
        /// Acepta el proceso de mantenimiento de Catálogos Carta Porte
        /// </summary>
        /// <returns>Estado de la aceptación del proceso</returns>
        [HttpPost]
        [RoleFilter(Roles: "AdministrationMaintenanceCartaPorte")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public async Task<IActionResult> Aceptar()
        {
            try
            {
                DataResult<CatalogosCartaPorteDto> dataResult = new DataResult<CatalogosCartaPorteDto>()
                {
                    User = _generals.User
                };

                DataResult<CatalogosCartaPorteDto> result = await _utility.Post(dataResult, "Catalogos/AceptarAsync");

                return Json(new { success = result.Status == System.Net.HttpStatusCode.OK, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al aceptar los Catálogos Carta Porte, por favor intente mas tarde." });
            }
        }

        /// <summary>
        /// Desarga el Catálogos Carta Porte en Excel
        /// </summary>
        /// <returns>Archivo XLSX</returns>
        [HttpGet]
        [RoleFilter(Roles: "AdministrationMaintenanceCartaPorte")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public async Task<IActionResult> Descargar()
        {
            try
            {
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:CatalogosCartaPorteBitacora").Value);

                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("pageNumber", 0));
                param.Add(new CustomHttpParameter("pageSize", 0));
                param.Add(new CustomHttpParameter("esDescarga", true));

                var result = await _utility.GetItem<DataResult<IEnumerable<CatalogosCartaPorteDto>>>("Catalogos/BitacoraSeleccionarAsync", param);



                if (result.Status != System.Net.HttpStatusCode.OK)
                    return Json(new { success = false, message = "Ocurrió un error al descargar la Bitácora Carta Porte, por favor intente mas tarde." });

                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Bitacora");
                worksheet.Cell(1, 1).Value = "Usuario";
                worksheet.Cell(1, 2).Value = "Descripcion";
                worksheet.Cell(1, 3).Value = "Fecha";

                int index = 1;
                foreach (var item in result.Data)
                {
                    worksheet.Cell(index + 1, 1).Value = item.Name;
                    worksheet.Cell(index + 1, 2).Value = item.Descripcion;
                    worksheet.Cell(index + 1, 3).Value = item.Fecha;
                    index++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BitacoraCartaPorte" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("SeguimientoCopade");
            }
        }
    }
}
