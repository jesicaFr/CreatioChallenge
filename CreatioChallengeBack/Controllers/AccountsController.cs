using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CreatioChallengeBack.Dtos;
using CreatioChallengeBack.Interfaces;

namespace CreatioChallengeBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly ICreatioClient _creatioClient;

        public AccountsController(ICreatioClient creatioClient)
        {
            _creatioClient = creatioClient;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _creatioClient.GetAccountsAsync(search, page, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest("El payload es obligatorio.");
            }

            var result = await _creatioClient.CreateAccountAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
