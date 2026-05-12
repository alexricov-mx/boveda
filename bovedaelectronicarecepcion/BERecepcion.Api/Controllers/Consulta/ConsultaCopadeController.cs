using BERecepcion.Api.Filters;
using BERecepcion.Core.Consulta.Copades.Dto;
using BERecepcion.Core.Consulta.Dto;
using BERecepcion.Core.Consulta.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BERecepcion.Api.Extensions;
using System.Net;

namespace BERecepcion.Api.Controllers.Consulta
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class ConsultaCopadeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IConsultaCopadeRepository _consultacopadeRepository;

        public ConsultaCopadeController(IConfiguration configuration, IConsultaCopadeRepository consultacopadeRepository)
        {
            _configuration = configuration;
            _consultacopadeRepository = consultacopadeRepository;
        }


        [HttpGet("GetConsultaCopadeAsync")]
        [ProducesResponseType(typeof(DataResult<List<CopadeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetConsultaCopadeAsync(DateTime fechaInicial, DateTime fechaFinal, string UserID, bool esDescarga, string pageSize, string search = null, int pageNum = 1)
        {
            var result = await _consultacopadeRepository.GetConsultaCopadeAsync(fechaInicial, fechaFinal, UserID, esDescarga, pageSize, search, pageNum);
            return result.ToActionResult();
        }


        [HttpGet("GetListaFiltroConsultaCopadeAsync")]
        [ProducesResponseType(typeof(DataResult<List<CopadeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListaFiltroConsultaCopadeAsync(string CopadeID, string UserID, string pageSize, int pageNum = 1)
        {
            var result = await _consultacopadeRepository.GetListaFiltroConsultaCopadeAsync(CopadeID, UserID, pageSize, pageNum);
            return result.ToActionResult();
        }

        //[HttpGet("ExpedienteElectronico")]
        //[ProducesResponseType(typeof(ExpedienteEViewModel), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> ExpedienteElectronico(string SAPOrder,Guid CopadeID)
        //{
        //    try
        //    {
        //        return Ok(await _consultacopadeRepository.ExpedienteElectronico(SAPOrder, CopadeID));
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("ExpedienteElectronico: {error}", ex.ToString());
        //        return Problem(null, null, 500, "Error interno", null);
        //    }
        //}

        #region Seguimiento
        [HttpGet("Seguimiento")]
        [ProducesResponseType(typeof(ExpedienteEViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Seguimiento(Guid CopadeID)
        {
            var result = await _consultacopadeRepository.Seguimiento(CopadeID);
            return result.ToActionResult();
        }
        #endregion
    }
}
