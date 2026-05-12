using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Catalogos.Dto;
using BERecepcion.Core.Catalogos.Interfaces.Repositories;
using BERecepcion.Core.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers.Catalogos
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class CatalogosController : ControllerBase
    {
        private readonly ICartaPorteRepository _cartaPorteRepository;
        private readonly IBitacoraRepository _bitacoraRepository;

        public CatalogosController(ICartaPorteRepository catalogosRepository, IBitacoraRepository bitacoraRepository)
        {
            _cartaPorteRepository = catalogosRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        /// <summary>
        /// Select a las tablas de Catálogos Carta Porte
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="pageNumber">Numero de la pagina</param>
        /// <param name="pageSize">Tamaño de la pagina</param>
        /// <param name="filtroBusqueda">Por si se desea implementar la busqueda</param>
        /// <returns>Tabla a consultar</returns>
        [HttpGet("SeleccionarAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<CatalogosCartaPorteDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SeleccionarAsync(string tableName, int pageNumber = 1, int pageSize = 10, string filtroBusqueda = null)
        {
            try
            {
                DataResult<IEnumerable<CatalogosCartaPorteDto>> result = await _cartaPorteRepository.SeleccionarAsync(tableName, pageNumber, pageSize, filtroBusqueda);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("Seleccionar Catálogos Carta Porte: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        /// <summary>
        /// Verifica que el proceso no se esté corriendo e inserta a las tablas Catálogos Carta Porte de paso
        /// </summary>
        /// <returns>Estado del proceso</returns>
        [HttpPost("CargarAsync")]
        [ProducesResponseType(typeof(DataResult<CatalogosCartaPorteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CargarAsync(DataResult<CatalogosCartaPorteDto> dataResult)
        {
            try
            {
                if (CatalogosCartaPorte.CatalogosCartaPorte.isRunning)
                {
                    dataResult = await _cartaPorteRepository.EnviarCorreo(dataResult, "Su archivo de Catálogos Carta Porte no fue procesado debido a que otro archivo se encuentra en proceso.");

                    return Problem(null, null, 500, "Is running", null);
                }

                dataResult = CatalogosCartaPorte.CatalogosCartaPorte.CargarXLS(dataResult, _cartaPorteRepository);

                if (dataResult.Status == System.Net.HttpStatusCode.OK)
                {
                    BitacoraDto bitacora = new BitacoraDto
                    {
                        UserID = dataResult.User.UserID,
                        Seccion = "Catálogos Carta Porte",
                        Accion = "Cargar",
                        Descripcion = string.Concat("Se cargan los Catálogos Carta Porte")
                    };

                    await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
                }

                dataResult = await _cartaPorteRepository.EnviarCorreo(dataResult, "Su archivo de Catálogos Carta Porte fue procesado correctamente, puede proceder a aceptar los cambios.");

                return Ok(dataResult);
            }
            catch (Exception ex)
            {
                await _cartaPorteRepository.EnviarCorreo(dataResult, "Ocurrio un problema al procesar su archivo de Catálogos Carta Porte. Contacta a tu administrador.");

                Log.Error("Cargar Catálogos Carta Porte: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        /// <summary>
        /// Valida las tablas Catálogos Carta Porte de paso
        /// </summary>
        /// <returns>Estado de la validación</returns>
        [HttpPost("ValidarAsync")]
        [ProducesResponseType(typeof(DataResult<CatalogosCartaPorteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidarAsync(DataResult<CatalogosCartaPorteDto> dataResult)
        {
            try
            {
                dataResult = await _cartaPorteRepository.ValidarAsync(dataResult);

                return Ok(dataResult);
            }
            catch (Exception ex)
            {
                Log.Error("Validar Catálogos Carta Porte: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        /// <summary>
        /// Acepta el proceso de mantenimiento de Catálogos Carta Porte
        /// </summary>
        /// <returns>Estado de la aceptación del proceso</returns>
        [HttpPost("AceptarAsync")]
        [ProducesResponseType(typeof(DataResult<CatalogosCartaPorteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AceptarAsync(DataResult<CatalogosCartaPorteDto> dataResult)
        {
            try
            {
                dataResult = await _cartaPorteRepository.AceptarAsync(dataResult);

                if (dataResult.Status == System.Net.HttpStatusCode.OK)
                {
                    BitacoraDto bitacora = new BitacoraDto
                    {
                        UserID = dataResult.User.UserID,
                        Seccion = "Catálogos Carta Porte",
                        Accion = "Aceptar",
                        Descripcion = string.Concat("Se aceptan los Catálogos Carta Porte")
                    };

                    await _bitacoraRepository.InsertaBitacoraAsync(bitacora);
                }

                return Ok(dataResult);
            }
            catch (Exception ex)
            {
                Log.Error("Aceptar Catálogos Carta Porte: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }

        /// <summary>
        /// Select a la tabla de Bitacora Catálogos Carta Porte
        /// </summary>
        /// <param name="pageNumber">Numero de la pagina</param>
        /// <param name="pageSize">Tamaño de la pagina</param>
        /// <param name="esDescarga">Descarga la tabla completa o la paginación</param>
        /// <returns>Tabla de bitacora</returns>
        [HttpGet("BitacoraSeleccionarAsync")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<CatalogosCartaPorteDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BitacoraSeleccionarAsync(int pageNumber = 1, int pageSize = 10, bool esDescarga = false)
        {
            try
            {
                DataResult<IEnumerable<CatalogosCartaPorteDto>> result = await _cartaPorteRepository.BitacoraSeleccionarAsync(pageNumber, pageSize, esDescarga);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("Seleccionar Bitacora Catálogos Carta Porte: {error}", ex.ToString());
                return Problem(null, null, 500, "Error interno", null);
            }
        }
    }
}
