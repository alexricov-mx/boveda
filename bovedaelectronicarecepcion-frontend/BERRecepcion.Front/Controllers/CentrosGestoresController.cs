using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using BERRecepcion.Front.Utilities;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class CentrosGestoresController : Controller
    {
        private readonly ILogger<CentrosGestoresController> _logger;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        protected readonly IConfiguration _configuration;

        public CentrosGestoresController(ILogger<CentrosGestoresController> logger, IRestUtility utility, IGenerals generals, IConfiguration configuration)
        {
            _logger = logger;
            _utility = utility;
            _generals = generals;
            _configuration = configuration;
        }

        [RoleFilter(Roles: "AdministrationManagementCenters")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CentrosGestoresTable(int pageNum = 1, string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:CentrosGestores").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                param.Add(new CustomHttpParameter("search", search));

                var result = await _utility.GetItem<DataResult<IEnumerable<ManagementCentersDto>>>("CentrosGestores/GetCGAsync", param);
                
                if (result.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    result.Pager = new Pager(result?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={result.Pager.TotalItems}");
                    result.Pager = new Pager(result.Pager.TotalItems, pageNum, pageSize);
                }
                ViewBag.Search = search;
                return PartialView("_CGTable", result);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al cargar los centros gestores, por favor intente mas tarde." });
            }
        }
        public async Task<IActionResult> ShowModal(Guid? ManagementCenterID = null, bool isEdit = false)
        {
            try
            {
                DataResult<ManagementCentersDto> result = null;
                if (ManagementCenterID != null)
                {
                    var param = new List<CustomHttpParameter>();
                    param.Add(new CustomHttpParameter("ManagementCenterID", ManagementCenterID));
                    result = await _utility.GetItem<DataResult<ManagementCentersDto>>("CentrosGestores/GetCGByIdAsync", param);
                    if (result.Status != System.Net.HttpStatusCode.OK || result.Data == null)
                        return Json(new { Success = false, Message = result.Message });
                }
                var orgDat = await _utility.GetItem<DataResult<IEnumerable<OrganismDto>>>("Catalogos");
                ViewBag.Organismos = orgDat.Data;
                ViewBag.IsEdit = isEdit;
                return PartialView("_ModalContent", result?.Data);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al cargar la información, por favor intente mas tarde" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Agregar(ManagementCentersDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = HandleModelState.GetErrors(ModelState) });
                var dataResult = new DataResult<ManagementCentersDto>
                {
                    Data = dto,
                    User = _generals.User
                };
                dto.UsuarioModificador = _generals.User.Token;
                var result = await _utility.Post(dataResult, "CentrosGestores/InsertaCGAsync");
                return Json(new { success = result.Status == System.Net.HttpStatusCode.OK, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al crear el centro gestor, por favor intente más tarde" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Editar(ManagementCentersDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = HandleModelState.GetErrors(ModelState) });
                var dataResult = new DataResult<ManagementCentersDto>
                {
                    Data = dto,
                    User = _generals.User
                };
                dto.UsuarioModificador = _generals.User.Token;
                var result = await _utility.Post(dataResult, "CentrosGestores/ActualizaCGAsync");
                return Json(new { success = result.Status == System.Net.HttpStatusCode.OK, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al actualizar el centro gestor, por favor intente más tarde" });
            }
        }
        public async Task<IActionResult> Eliminar(Guid ManagementCenterID)
        {
            try
            {
                var dataResult = new DataResult<ManagementCentersDto>
                {
                    Data = new ManagementCentersDto { ManagementCenterID = ManagementCenterID },
                    User = _generals.User
                };
                var result = await _utility.Post(dataResult, "CentrosGestores/BorraCGAsync");
                return Json(new { success = result.Status == System.Net.HttpStatusCode.OK, message = result.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al eliminar el centro gestor, por favor intente mas tarde" });
            }
        }
        public async Task<IActionResult> DownloadExcel(string search = null)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:CentrosGestores").Value);
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("search", search));
                param.Add(new CustomHttpParameter("esDescarga", true));
                var centgest = await _utility.GetItem<DataResult<IEnumerable<ManagementCentersDto>>>("CentrosGestores/GetCGAsync", param);
                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("CentrosGestores");
                worksheet.Cell(1, 1).Value = "Centro";
                worksheet.Cell(1, 2).Value = "Descripcion";
                worksheet.Cell(1, 3).Value = "Firmante1";
                worksheet.Cell(1, 4).Value = "Suplente1";
                worksheet.Cell(1, 5).Value = "Firmante2";
                worksheet.Cell(1, 6).Value = "Suplente2";
                worksheet.Cell(1, 7).Value = "Tipo";
                worksheet.Cell(1, 8).Value = "Clave";

                var cg = centgest.Data.ToList();
                for (int index = 1; index <= cg.Count; index++)
                {
                    worksheet.Cell(index + 1, 1).Value = cg[index - 1].Number;
                    worksheet.Cell(index + 1, 2).Value = cg[index - 1].Description;
                    worksheet.Cell(index + 1, 3).Value = cg[index - 1].Signer1;
                    worksheet.Cell(index + 1, 4).Value = cg[index - 1].Alternate1;
                    worksheet.Cell(index + 1, 5).Value = cg[index - 1].Signer2;
                    worksheet.Cell(index + 1, 6).Value = cg[index - 1].Alternate2;
                    worksheet.Cell(index + 1, 7).Value = cg[index - 1].Type;
                    worksheet.Cell(index + 1, 8).Value = cg[index - 1].OrganismID.ToString();
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CentroGestorAll" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return RedirectToAction("Index");
            }
        }









        public IActionResult CargarExcel()
        {
            try
            {
                return PartialView("_CargarExcel");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public IActionResult verificarCargaCentrosGestoresExcel(string CentrosGestoresList)
        {

            try
            {
                IEnumerable<ManagementCentersDto> CentrosGestores = JsonConvert.DeserializeObject<IEnumerable<ManagementCentersDto>>(CentrosGestoresList);
                if (CentrosGestores.Count() != 0)
                {
                    return Json(new { Success = true, Message = "El archivo fue cargado con éxito" });
                    //return Ok(CentrosGestores);
                }
                return Json(new { Success = false, Message = "No se ha seleccionado ningún archivo." });
                //return Json(new { Success = false, Message = "El archivo cargado tiene campos null o vacios" })

            }
            catch (Exception)
            {
                // mensaje error, todas las renglones deben traer un valor
                throw;
            }
        }
        public async Task<IActionResult> cargarCentrosGestoresBD(string CentrosGestoresList)
        {
            try
            {
                if (CentrosGestoresList != " JSON : ")
                {
                    IEnumerable<ManagementCentersDto> CentrosGestores = JsonConvert.DeserializeObject<IEnumerable<ManagementCentersDto>>(CentrosGestoresList);

                    if (CentrosGestores.Count() != 0)
                    {
                        for (int i = 0; i < CentrosGestores.Count(); i++)
                        {
                            CentrosGestores.ElementAt(i).UsuarioModificador = _generals.User.Token;
                        }

                        DataResult<IEnumerable<ManagementCentersDto>> CGList = new DataResult<IEnumerable<ManagementCentersDto>>() { Data = CentrosGestores };

                        var result = await _utility.Post(CGList, "CentrosGestores/CargaCentrosGAsync");
                        if (result.Status == System.Net.HttpStatusCode.OK)
                            return Json(new { Success = true, Message = result.Message });
                        else
                            return Json(new { Success = false, Message = result.Message });
                    }
                }
                return Json(new { Success = false, Message = "Solo puedes cargar si hay un archivo seleccionado" });

            }
            catch (Exception)
            {
                // mensaje error, todas las renglones deben traer un valor
                throw;
            }
        }
        

        public async Task<IActionResult> BusquedaCGUsuario(string ficha)
        {
            try
            {
                if (ficha == null)
                {
                    ficha = "";
                }
                var datos = await _utility.GetItem<DataResult<IEnumerable<ManagementCentersDto>>>("CentrosGestores/BusquedaCGUsuarioAsync/" + ficha);

                string Nombre = "";
                for (int i = 0; i < datos.Data.Count(); i++)
                {
                    //Nombre = datos.Data.ElementAt(i).Name;
                }

                return Json(new { Success = true, Nombre });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
