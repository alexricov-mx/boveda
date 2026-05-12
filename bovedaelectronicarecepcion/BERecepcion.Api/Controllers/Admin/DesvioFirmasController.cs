using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
using BERecepcion.Core.Correos.Interfaces.Repositories;
using BERecepcion.Core.Dto;
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
    public class DesvioFirmasController : ControllerBase
    {
        private readonly IDesvioFirmasRepository _desvioFirmasRepository;
        private readonly IBitacoraAdmonRepository _bitacoraAdmonRepository;
        private readonly ICorreoRepository _correoRepository;
        private readonly IUsuariosRepository _usuariosRepository;
        public DesvioFirmasController(IDesvioFirmasRepository desvioFirmasRepository, IBitacoraAdmonRepository bitacoraAdmonRepository,
            ICorreoRepository correoRepository, IUsuariosRepository usuariosRepository)
        {
            _desvioFirmasRepository = desvioFirmasRepository;
            _bitacoraAdmonRepository = bitacoraAdmonRepository;
            _correoRepository = correoRepository;
            _usuariosRepository = usuariosRepository;
        }

        [HttpGet("pendientes/{contrato}")]
        [ProducesResponseType(typeof(DataResult<DesvioPendientesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDesvioPendientesAsync(string contrato)
        {
            var result = await _desvioFirmasRepository.GetDesvioPendientesAsync(contrato);
            return result.ToActionResult();
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<List<RealizaDesvioResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostRealizaDesvioFAsync(RealizaDesvioFirmasDto desvio)
        {
            var result = await _desvioFirmasRepository.PostRealizaDesvioFAsync(desvio);

            string Correo, UserName;

            List<BitacoraDesvioFirmaDto> desvios = new List<BitacoraDesvioFirmaDto>() { };

            if (desvio.Contrato != null)
            {
                BitacoraDesvioFirmaDto b = new BitacoraDesvioFirmaDto();
                b.Signer = desvio.Contrato.Signer;
                b.SignerNew = desvio.Contrato.SignerNew;
                b.Documento = desvio.Contrato.Contract;
                b.UsuarioModificador = desvio.Contrato.UsuarioModificador;
                desvios.Add(b);
            }
            if (desvio.EstimacionObra != null)
            {
                foreach (var item in desvio.EstimacionObra)
                {
                    BitacoraDesvioFirmaDto b = new BitacoraDesvioFirmaDto();
                    b.Signer = item.Signer;
                    b.SignerNew = item.SignerNew;
                    b.UsuarioModificador = item.UsuarioModificador;
                    b.Documento = item.Contract;
                    desvios.Add(b);
                }
            }
            if (desvio.OrdenSurtimiento != null)
            {
                foreach (var item in desvio.OrdenSurtimiento)
                {
                    BitacoraDesvioFirmaDto b = new BitacoraDesvioFirmaDto();
                    b.Signer = item.Signer;
                    b.SignerNew = item.SignerNew;
                    b.UsuarioModificador = item.UsuarioModificador;
                    b.Documento = item.Contract;
                    desvios.Add(b);
                }
            }
            if (desvio.Recepcion != null)
            {
                BitacoraDesvioFirmaDto b = new BitacoraDesvioFirmaDto();
                foreach (var item in desvio.Recepcion)
                {
                    b.Signer = item.Signer;
                    b.SignerNew = item.SignerNew;
                    b.UsuarioModificador = item.UsuarioModificador;
                    b.Documento = item.Contract;
                    desvios.Add(b);
                }
            }

            foreach (var item in desvios)
            {
                Correo = UserName = "";

                if (!string.IsNullOrEmpty(item.UsuarioModificador))
                {
                    var bitacora = new BitacoraAdmonDto
                    {
                        Evento = "Nuevo",
                        Usuario = item.UsuarioModificador,
                        Descripcion = "Se modifico la firma de " + item.Documento + " - " + item.Signer + " -- Nuevo Usuario -" + item.SignerNew
                    };

                    if (result.Status != HttpStatusCode.OK)
                    {
                        bitacora.Descripcion = "Error al modificar la firma de " + item.Documento + " - " + item.Signer + " -- Nuevo Usuario -" + item.SignerNew;
                    }
                    await _bitacoraAdmonRepository.InsertaBitacoraAdmonAsync(bitacora);

                    var userList = _usuariosRepository.GetUsuariosByToken(item.UsuarioModificador);
                    if (userList.Result.Data != null)
                    {
                        var user = userList.Result.Data.FirstOrDefault();
                        if (user != null)
                        {
                            Correo = user.Email;
                            UserName = user.UserName;
                            if (!string.IsNullOrEmpty(Correo))
                            {
                                await _correoRepository.NotificacionDesvioFirma(Correo, UserName, result.Status, item.Documento, item.Signer, item.SignerNew);
                            }
                        }
                    }
                }
            }

            return result.ToActionResult();
        }
    }
}
