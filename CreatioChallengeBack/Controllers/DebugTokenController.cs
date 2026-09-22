using System;
using System.Threading.Tasks;
using CreatioChallengeBack.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CreatioChallengeBack.Controllers
{
    [ApiController]
    [Route("api/debug-token")]
    public class DebugTokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<DebugTokenController> _logger;

        public DebugTokenController(ITokenService tokenService, ILogger<DebugTokenController> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        // GET api/debug-token?raw=true
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool raw = false)
        {
            try
            {
                var token = await _tokenService.GetAccessTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(token))
                    return StatusCode(500, new { success = false, message = "No se obtuvo token." });

                if (raw)
                {
                    // Return full token only when explicitly requested
                    return Ok(new { success = true, token });
                }

                // Mask token for safety: show first 10 and last 8 characters
                var masked = token.Length <= 20 ? new string('*', token.Length) : token.Substring(0, 10) + "..." + token.Substring(token.Length - 8);
                return Ok(new { success = true, token = masked });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining token for debug endpoint");
                return StatusCode(500, new { success = false, message = "Error al obtener token." });
            }
        }
    }
}
