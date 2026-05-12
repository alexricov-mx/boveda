using BERecepcion.Api.Extensions;
using BERecepcion.Api.Filters;
using BERecepcion.Core.Admin.Dto;
using BERecepcion.Core.Admin.Interfaces.Repositories;
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
    public class AdmonGRMController : ControllerBase
    {
        private readonly IAdmonGRMRepository _admonGRMRepository;
        public AdmonGRMController(IAdmonGRMRepository admonGRMRepository)
        {
            _admonGRMRepository = admonGRMRepository;
        }

        [HttpPost]
        [ProducesResponseType(typeof(DataResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertaFirmaGRMAsync(AdmonGRMDto admonGRMDto)
        {
            var result = await _admonGRMRepository.InsertaFirmaGRMAsync(admonGRMDto);
            return result.ToActionResult();
        }

        [HttpGet("{AdministratorToken}/{pageNum}/{pageSize}")]
        [ProducesResponseType(typeof(DataResult<IEnumerable<AdmonGRMDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFirmasGRMAsync(string AdministratorToken, string pageNum, string pageSize)
        {
            int tmpPageNum = 1;
            int tmpPageSize = 1;
            try
            {
                tmpPageNum = int.Parse(pageNum);
            }
            catch { }
            try
            {
                tmpPageSize = int.Parse(pageSize);
            }
            catch { }
            
            var result = await _admonGRMRepository.GetFirmasGRMAsync(AdministratorToken, tmpPageNum, tmpPageSize);
            return result.ToActionResult();
        }

    }
}
