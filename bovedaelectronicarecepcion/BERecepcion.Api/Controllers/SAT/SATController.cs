using BERecepcion.Api.Filters;
using BERecepcion.Core.Dto;
using BERecepcion.Core.SAT.Dto;
using BERecepcion.Core.SAT.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.SAT
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class SATController : ControllerBase
    {
        private readonly ISATRepository _satRepository;
        public SATController(ISATRepository sATRepository)
        {
            _satRepository = sATRepository;
        }

        [HttpGet("GetValidacionSATAsync/{RE}/{RR}/{TT}/{UUID}")]
        [ProducesResponseType(typeof(DataResult<ValidacionSATDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<DataResult<ValidacionSATDto>> GetValidacionSATAsync(string RE, string RR, string TT, Guid UUID)
        {
            DataResult<ValidacionSATDto> result = new DataResult<ValidacionSATDto>();
            ValidacionSATDto validacionSAT = new ValidacionSATDto()
            {
                RFCEmisor = RE,
                RFCReceptor = RR,
                Total = TT,
                UUID = UUID
            };
            try
            {
                result = await _satRepository.GetValidacionSATAsync(validacionSAT);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error("ValidacionSAT: GetValidacionSATAsync {error}", ex.ToString());
                return result;
            }
        }

        [HttpPost("RecepcionCFDI")]
        [ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecepcionCFDI(IFormFile archivoCFDI)
        {
            try
            {
                return Ok(await _satRepository.validaCFDI(archivoCFDI));
            }
            catch (Exception ex)
            {
                Log.Error("RecepcionCFDI: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        #region Catalogos SAT
        [HttpPost("Catalogos/GetCartaPorteDireccionesAsync")]
        [ProducesResponseType(typeof(DataResult<CartaPorte>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDireccionAsync(DataResult<CartaPorte> dto)
        {
            try
            {
                foreach (var item in dto.Data.Ubicaciones)
                {
                    var domicilioCompleto = await _satRepository.GetDireccionAsync(item.Domicilio.CodigoPostal, item.Domicilio.Municipio, item.Domicilio.Localidad, item.Domicilio.Estado);
                    item.Domicilio.DomicilioCompleto = domicilioCompleto.Data;
                }
                return Ok(dto);
            }
            catch (Exception ex)
            {
                Log.Error("GetDireccionAsync: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        //[HttpPost("Catalogos/GetDireccionAsync")]
        //[ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GetDireccionAsync(string CodigoPostal, string Municipio, string Localidad, string Estado)
        //{
        //    try
        //    {
        //        return Ok(await _satRepository.GetDireccionAsync(CodigoPostal, Municipio, Localidad, Estado));
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("GetDireccionAsync: {error}", ex.ToString());
        //        return Problem(null, null, 500, "Error interno", null);
        //    }
        //}
        #endregion
    }
}
