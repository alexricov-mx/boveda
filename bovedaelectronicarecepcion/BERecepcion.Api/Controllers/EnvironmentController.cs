using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BERecepcion.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        public EnvironmentController(IHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(new Dictionary<string, object>
            {
                ["applicationName"] = _environment.ApplicationName,
                ["environmentName"] = _environment.EnvironmentName,
                ["isDevelopment"] = _environment.IsDevelopment(),
                ["isStaging"] = _environment.IsStaging(),
                ["isProduction"] = _environment.IsProduction(),
                ["Settings"] = new Dictionary<string, object>
                {
                    ["ConnectionStrings:SQLServerSQLDEV002"] = _configuration["ConnectionStrings:SQLServerSQLDEV002"],
                    ["eSign:EndpointNet6:url"] = _configuration["eSign:EndpointNet6:url"]                   
                }
            });
        }
    }
}
