using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class InterfacesController : ControllerBase
    {

        private readonly IInterfacesRepository _interfacesRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        public InterfacesController(IInterfacesRepository interfacesRepository, IBitacoraRepository bitacoraRepository)
        {
            _interfacesRepository = interfacesRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        [HttpGet("GetInterfacesAsync")]
        [ProducesResponseType(typeof(ControlInterfacesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInterfacesAsync()
        {
            var result = await _interfacesRepository.GetInterfacesAsync();
            return result.ToActionResult();
        }
        [HttpGet("GetControlInterfacesRolesAsync")]
        [ProducesResponseType(typeof(ControlInterfacesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetControlInterfacesRolesAsync(Guid sapId)
        {
            var result = await _interfacesRepository.GetControlInterfacesRolesAsync(sapId);
            return result.ToActionResult();
        }



        [HttpPost("ActualizarAsync")]
        [ProducesResponseType(typeof(DataResult<ControlInterfacesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarAsync([FromBody] DataResult<ControlInterfacesDto> dto)
        {
            dto.Data.ControlInterfacesDetail.UsuarioModif = dto.User.Token;
            var result = await _interfacesRepository.ActualizarAsync(dto.Data);
            if (result.Status == HttpStatusCode.OK)
            {
                var bitacora = new BitacoraDto
                {
                    UserID = dto.User.UserID,
                    Seccion = "Interfaces",
                    Accion = dto.Data.Status ? "Activa" : "Desactiva",
                    Descripcion = string.Concat(dto.Data.Status ? "Se Activa SAP - " : "Se Desactiva SAP - ", dto.Data.SAP)
                };

                await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
            }

            return result.ToActionResult();
        }



        //[HttpPost("ActivaDesactivaTodoInterface")]
        //[ProducesResponseType(typeof(DataResult<ControlInterfacesDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> ActivaDesactivaTodoInterfaceAsync([FromBody] DataResult<ControlInterfacesDto> dto)
        //{
        //    try
        //    {

        //        var result = await _interfacesRepository.ActivaDesactivaTodoInterfaceAsync(dto.Data);
        //        if (result.Status == System.Net.HttpStatusCode.OK)
        //        {
        //            var bitacora = new BitacoraDto
        //            {
        //                UserID = dto.User.UserID,
        //                Seccion = "ActivaDesactivaTodoInterface",
        //                Accion = (dto.Data.Status) ? "Activa" : "Desactiva",
        //                Descripcion = string.Concat((dto.Data.Status) ? "Se Activa SAP - " : "Se Desactiva SAP - ", dto.Data.usuario)
        //            };

        //            await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
        //        }

        //        result.Message = (dto.Data.Status) ? "Se Activa SAP" : "Se Desactiva SAP";

        //        return Ok(result);

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("ActivaDesactivaTodoInterface: Actualiza Todo Intefaces {error}", ex.ToString());
        //        return Problem(null, null, 500, "Error interno", null);
        //    }
        //}
    }
}
