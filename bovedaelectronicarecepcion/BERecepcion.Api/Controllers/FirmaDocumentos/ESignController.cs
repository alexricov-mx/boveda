using BERecepcion.Api.Filters;
using BERecepcion.Api.ModelBinding;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Copades.Interfaces.Repositories;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using BERecepcion.Core.eSignDto;
using BERecepcion.Core.FirmaDocumentos.Dto;
using BERecepcion.Core.FirmaDocumentos.Interfaces.Repositories;
using BERecepcion.Core.OrdenSurtimiento.Interfaces.Repositories;
using BERecepcion.Core.SAPPI.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.FirmaDocumentos
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class ESignController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IESignRepository _eSignRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        private readonly ICorreoRepository _correoRepository;
        private readonly IDocumentosRepository _documentosRepository;
        private readonly IHostEnvironment _env;
        private readonly ISupplyOrderRepository _supplyOrderRepository;
        private readonly ICopadeRepository _copadeRepository;//add
        private readonly ISAPPIRepository _sAPPIRepository;
        private readonly IUsuariosRepository _usuariosRepository;
        public ESignController(IConfiguration configuration, IESignRepository eSignRepository, IBitacoraRepository bitacoraRepository, ICorreoRepository correoRepository, IDocumentosRepository documentosRepository, IHostEnvironment env, ISupplyOrderRepository supplyOrderRepository, ISAPPIRepository sAPPIRepository, IUsuariosRepository usuariosRepository)
        {
            _configuration = configuration;
            _eSignRepository = eSignRepository;
            _bitacoraRepository = bitacoraRepository;
            _correoRepository = correoRepository;
            _documentosRepository = documentosRepository;
            _sAPPIRepository = sAPPIRepository;
            _env = env;
            _supplyOrderRepository = supplyOrderRepository;
            _usuariosRepository = usuariosRepository;
        }
        [HttpPost("ValidaOCreaUsuarios")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostValidaOCreaUsuarioAsync([FromBody] DataResult<UsuarioDto> usuario)
        {            
            try
            {
                usuario.Data.EstatusClave = int.Parse(_configuration["eSign:Usuarios:estatusClave:Activo"]);                
                usuario.Data.UsuarioAlta = int.Parse(_configuration["eSign:Usuarios:usuarioAltaId"]);
                usuario.Data.usuarioRefOrg = new UnidadOrgUsuario(_configuration);
                return Ok(await _eSignRepository.GetValidaOCreaUsuarioAsync(usuario.Data));
            }
            catch (Exception ex)
            {
                Log.Error("Externos eSign: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
                
        [HttpPost("ConsultaEstadoOCSP")]
        [ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultaEstadoOCSP(IFormFile cerClientePath)
        {
            try
            {
                return Ok(await _eSignRepository.ConsultaEstadoOCSP(cerClientePath));
            }
            catch (Exception ex)
            {
                Log.Error("Externos eSign: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpPost("ValidaCertificado")]
        [ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidaCertificado([FromBody] certificadoDto reqCertificado)
        {
            try
            {
                return Ok(await _eSignRepository.ValidaCertificado(reqCertificado.Certificado));
            }
            catch (Exception ex)
            {
                Log.Error("Externos eSign: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        /// <summary>
        /// Metodo para obtner la evidencia de firma electronica del portal de firma eSign para poder mostrarlo en el expediente electronico
        /// de la Boveda
        /// </summary>
        /// <param name="PaqueteId"></param>
        /// <param name="UsuarioId"></param>
        /// <returns>
        /// DataResult<DocumentoFirma>
        /// </returns>
        [HttpGet("ConsultaDocumentoFirma")]
        [ProducesResponseType(typeof(DataResult<ExternosDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultaDocumentoFirmaAsync(int PaqueteId,int DocumentoId, int UsuarioId)
        {
            try
            {
                var docFirmado = await _eSignRepository.ConsultaDocumentoFirmaAsync(PaqueteId, DocumentoId, UsuarioId);
                docFirmado.User = await _usuariosRepository.GetUsuarioByESignId(UsuarioId);
                return Ok(docFirmado);
            }catch(Exception ex)
            {
                Log.Error("Externos ConsultaPaqueteDocumentoId: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("RecuperaPDFFirmadoAsync/{PaqueteId}/{DocumentoId}")]
        [ProducesResponseType(typeof(DataResult<ArchivoPDFDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecuperaPDFFirmadoAsync(int PaqueteId, int DocumentoId)
        {
            try
            {
                return Ok(await _eSignRepository.RecuperaPDFFirmado(PaqueteId,DocumentoId));
            }
            catch (Exception ex)
            {
                Log.Error("Externos ConsultaPaqueteDocumentoId: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("RecuperaPDFAPFirmadoAsync/{PaqueteId}/{DocumentoId}")]
        [ProducesResponseType(typeof(DataResult<ArchivoPDFDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecuperaPDFAPFirmadoAsync(int PaqueteId, int DocumentoId)
        {
            try
            {
                return Ok(await _eSignRepository.RecuperaPDFAPFirmado(PaqueteId, DocumentoId));
            }
            catch (Exception ex)
            {
                Log.Error("Externos ConsultaPaqueteDocumentoId: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        [HttpGet("RecuperaPDFFirmadoEFirmaAsync/{IdCorrelacion}")]
        [ProducesResponseType(typeof(DataResult<ArchivoPDFDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RecuperaPDFFirmadoEFirmaAsync(Guid IdCorrelacion)
        {
            try
            {                
                return Ok(await _eSignRepository.RecuperaPDFFirmadoEFirmaAsync(IdCorrelacion));
            }
            catch (Exception ex)
            {
                Log.Error("Externos ConsultaPaqueteDocumentoId: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
    }
}
