using BERRecepcion.Front.Filters;
using BERRecepcion.Front.Interfaces;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BERRecepcion.Front.Controllers
{
    [ValidateUser]
    public class BitacoraController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IRestUtility _utility;
        private readonly IGenerals _generals;
        public BitacoraController(IConfiguration configuration, IRestUtility utility, IGenerals generals)
        {
            _configuration = configuration;
            _utility = utility;
            _generals = generals;
        }
        [RoleFilter(Roles: "AdministrationLog")]
        [UserTypeFilter("UserTypeS,UserTypeA")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Obtiene la lista de bitacora
        /// </summary>
        /// <param name="pageNum"></param>
        /// <returns>En caso de éxito, devuelve vista parcial con la tabla. En caso de error un Json con un mensaje genérico de error.</returns>
        [HttpGet]
        public async Task<IActionResult> BitacoraTable(DateTime fechaInicial, DateTime fechaFinal, string busqueda, int pageNum = 1)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                int pageSize = Convert.ToInt32(_configuration.GetSection("Paginacion:Bitacora").Value);
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("busqueda", busqueda));
                param.Add(new CustomHttpParameter("pageSize", pageSize));
                param.Add(new CustomHttpParameter("pageNum", pageNum));
                var bitacora = await _utility.GetItem<DataResult<IEnumerable<BitacoraDto>>>("Bitacora/GetBitacoraAsync", param);
                
                if (bitacora.Pager == null)
                {
                    Log.Warning("⚠️ Backend no retornó Pager, creando uno por defecto");
                    bitacora.Pager = new Pager(bitacora?.Data?.Count() ?? 0, pageNum, pageSize);
                }
                else
                {
                    Log.Information($"✓ Pager recibido: TotalItems={bitacora.Pager.TotalItems}");
                    bitacora.Pager = new Pager(bitacora.Pager.TotalItems, pageNum, pageSize);
                }
                ViewBag.EmptyResults = bitacora.Pager.TotalItems == 0;
                return PartialView("_BitacoraTable", bitacora);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return Json(new { Success = false, Message = "Ocurrió un error al cargar la bitacora, por favor intente mas tarde" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> DownloadExcel(DateTime fechaInicial, DateTime fechaFinal, string busqueda)
        {
            try
            {
                var param = new List<CustomHttpParameter>();
                param.Add(new CustomHttpParameter("fechaInicial", fechaInicial.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("fechaFinal", fechaFinal.ToString("yyyy-MM-dd")));
                param.Add(new CustomHttpParameter("busqueda", busqueda));
                var bitacora = await _utility.GetItem<DataResult<IEnumerable<BitacoraDto>>>("Bitacora/GetAllBitacoraAsync", param);
                var workbook = new XLWorkbook();
                IXLWorksheet worksheet = workbook.Worksheets.Add("Bitacora");
                worksheet.Cell(1, 1).Value = "UserName";
                worksheet.Cell(1, 2).Value = "Seccion";
                worksheet.Cell(1, 3).Value = "Accion";
                worksheet.Cell(1, 4).Value = "Descripcion";
                worksheet.Cell(1, 5).Value = "Fecha";
                var list = bitacora.Data.ToList();
                for (int index = 1; index <= list.Count; index++)
                {
                    worksheet.Cell(index + 1, 1).Value = list[index - 1].UserName;
                    worksheet.Cell(index + 1, 2).Value = list[index - 1].Seccion;
                    worksheet.Cell(index + 1, 3).Value = list[index - 1].Accion;
                    worksheet.Cell(index + 1, 4).Value = list[index - 1].Descripcion;
                    worksheet.Cell(index + 1, 5).Value = list[index - 1].Fecha.ToString("dd/MM/yyyy HH:mm:ss");
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Bitácora.xlsx");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }
    }
}
