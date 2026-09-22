using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CreatioChallengeBack.Interfaces;

namespace CreatioChallengeBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreatioProxyController : ControllerBase
    {
        private readonly ICreatioClient _creatioClient;

        public CreatioProxyController(ICreatioClient creatioClient)
        {
            _creatioClient = creatioClient;
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts(CancellationToken cancellationToken)
        {
            var list = await _creatioClient.GetContactsAsync(cancellationToken);
            return Ok(list);
        }
    }
}
