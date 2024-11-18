
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace VolumeControl.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        public AuthController(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        [HttpPost("login")]
        public IActionResult CheckAuthKey([FromBody] AuthToken token)
        {
            if (_appSettings.AuthKey == token.AuthKey)
            {
                return Ok(new { Success = true });
            }
            return StatusCode(403, new { Success = false });
        }
    }

    public class AuthToken
    {
        public string AuthKey { get; set; }
    }
}