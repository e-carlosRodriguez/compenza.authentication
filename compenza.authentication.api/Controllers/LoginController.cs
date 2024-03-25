using compenza.authentication.api.Payloads.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace compenza.authentication.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Validar([FromBody] LoginCredentialsRequest request)
        {
            return Ok("");
        }
    }
}
